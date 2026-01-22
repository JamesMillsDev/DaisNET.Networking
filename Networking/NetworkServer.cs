using System.Net;
using System.Net.Sockets;
using DaisNET.Networking.Packets;
using DaisNET.Utility;
using DaisNET.Utility.Extensions;

namespace DaisNET.Networking
{
	public class NetworkServer : Network
	{
		/// <summary>
		/// The next available ID to assign to a new client connection. Increments with each new connection.
		/// </summary>
		private static uint nextId;

		/// <summary>
		/// Flag indicating whether the server is in the process of shutting down.
		/// Used to stop accepting new connections during graceful shutdown.
		/// </summary>
		private bool isClosing;

		/// <summary>
		/// The maximum number of pending connections that can be queued.
		/// </summary>
		private readonly int connectionBacklogSize;

		/// <summary>
		/// List of all currently connected client sockets.
		/// Thread-safe access is enforced using locks.
		/// </summary>
		private readonly List<Socket> connections = [];

		/// <summary>
		/// Queue of IDs that have been returned (from disconnected clients) and can be reused.
		/// </summary>
		private readonly Queue<uint> returnedIds = [];

		/// <summary>
		/// Queue of sockets awaiting ID assignment paired with their assigned IDs.
		/// Used to track pending client connections before they're fully initialized.
		/// </summary>
		private readonly Queue<Tuple<Socket, uint>> queuedIds = [];

		/// <summary>
		/// Initializes a new network server instance and starts the connection acceptance loop.
		/// Sets <see cref="Network.HasAuthority"/> to true as the server has authoritative control.
		/// </summary>
		/// <param name="host">The hostname or IP address to bind the server to.</param>
		/// <param name="port">The port number to listen on. Defaults to 25565.</param>
		/// <param name="backlog">The amount of pending connections that can be listening for.</param>
		public NetworkServer(string host, int port = 25565, int backlog = 10) : base(host, port)
		{
			// Server has authoritative control over game state
			this.HasAuthority = true;

			// Copy the connection backlog count
			this.connectionBacklogSize = backlog;

			// Start the connection acceptance loop in the background
			_ = Task.Run(async () =>
			{
				try
				{
					await AwaitConnections();
				}
				catch (Exception e)
				{
					Console.WriteLine($"NetworkServer - Connection Loop:Exception caught: {e}");
				}
			});
		}

		/// <summary>
		/// Sends a packet to all connected clients.
		/// Thread-safe operation that broadcasts the same packet to every client in the connections list.
		/// </summary>
		/// <param name="packet">The packet to serialize and broadcast to all clients.</param>
		public void BroadcastPacket(Packet packet)
		{
			lock (this.connections)
			{
				foreach (Socket connection in this.connections)
				{
					SendPacket(packet, connection);
				}
			}
		}

		/// <summary>
		/// Continuously attempts to handle packets that are received from each and any client.
		/// </summary>
		protected override async Task Poll()
		{
			// Create a snapshot of connected clients to avoid holding the lock during async operations
			List<Socket> connected = [];
			lock (this.connections)
			{
				connected.AddRange(this.connections);
			}

			// Start reading packets from all connected clients simultaneously
			List<Task<Tuple<string, byte[]>>> reading = new(connected.Select(ReadPacket));
			await Task.WhenAll(reading.ToArray());

			// Process each received packet
			foreach ((string id, byte[] payload) in reading.Select(task => task.Result))
			{
				// Skip packets with invalid or unregistered IDs
				if (!TryMakePacketFor(id, out Packet? packet))
				{
					continue;
				}

				// Deserialize the packet payload and process it
				PacketReader reader = new(payload);
				packet!.Deserialize(reader);
				await packet.Process();
			}

			// Copy the queue clearing the original.
			Queue<Tuple<Socket, uint>> queued;
			lock (this.queuedIds)
			{
				queued = new Queue<Tuple<Socket, uint>>(this.queuedIds);
				this.queuedIds.Clear();
			}

			// Iterate over all queued items
			while (queued.Count > 0)
			{
				(Socket connectedSocket, uint connectedId) = queued.Dequeue();

				SendPacket(
					new ConnectionPacket(true, connectedId, $"{connectedSocket.RemoteEndPoint}"),
					connectedSocket
				);

				// Add the remote player to our player list
				this.players.Add(new NetworkPlayer
					{
						Connection = new NetworkConnection(IPEndPoint.Parse($"{connectedSocket.RemoteEndPoint}"))
						{
							ID = connectedId
						},
						IsLocalPlayer = false
					}
				);

				// Iterate over each connected client that isn't the queued one
				foreach (Socket connection in connected.Where(conn => conn != connectedSocket))
				{
					byte connectionIndex = (byte)connected.IndexOf(connection);

					// Send the connection packet for the new connection to the old ones
					SendPacket(
						new ConnectionPacket(false, connectedId, $"{connectedSocket.RemoteEndPoint}"),
						connection
					);

					// Send the connection packet for the old connection to the new one
					SendPacket(
						new ConnectionPacket(false, connectionIndex, $"{connection.RemoteEndPoint}"),
						connectedSocket
					);
				}
			}

			lock (this.connections)
			{
				// Find all connections that have disconnected
				List<Socket> toDisconnect = this.connections.Where(con => !con.IsConnected()).ToList();
				foreach (Socket connection in toDisconnect)
				{
					// Remove from the active connections list
					this.connections.Remove(connection);

					// Shut down the socket in both directions (send and receive) then close / release resources
					connection.Shutdown(SocketShutdown.Both);
					connection.Close();

					// Check if there is any players with the correct ip
					if (this.players.All(p => p.Connection.IP != connection.RemoteEndPoint))
					{
						continue;
					}
					
					NetworkPlayer player = this.players.First(p => p.Connection.IP == connection.RemoteEndPoint);

					// Return the id for this connection and remove it from the player list
					this.returnedIds.Enqueue(player.Connection.ID);
					this.players.Remove(player);
				}
			}

			// Wait for the configured poll interval before checking for more packets
			await Task.Delay(this.pollRate);
		}

		/// <summary>
		/// Binds to the local endpoint and starts listening.
		/// </summary>
		protected override void Open()
		{
			this.socket = new Socket(this.ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.socket.Bind(this.targetEndPoint);
			this.socket.Listen(this.connectionBacklogSize);
		}

		/// <summary>
		/// Closes all client connections and shuts down the server.
		/// Gracefully disconnects each connected client before clearing the connection list.
		/// </summary>
		protected override void Close()
		{
			lock (this.connections)
			{
				// Disconnect each client from the server
				foreach (Socket connection in this.connections)
				{
					connection.Shutdown(SocketShutdown.Both);
					connection.Close();
				}

				// Clear the connection list
				this.connections.Clear();
			}

			// Signal that the server is shutting down to stop accepting new connections
			this.isClosing = true;
		}

		/// <summary>
		/// Continuously accepts incoming client connections until the server is closing.
		/// Waits for the socket to be initialized, then enters an accept loop that assigns IDs to new clients.
		/// </summary>
		private async Task AwaitConnections()
		{
			// Wait until the server socket is initialized
			await Tasks.While(() => this.socket == null);

			// Continue accepting connections until server shutdown
			while (!this.isClosing)
			{
				// Begin accepting a client connection
				Task<Socket> clientSocket = this.socket!.AcceptAsync();
				// Wait for either a client to connect or the server to signal shutdown
				await Task.WhenAny(clientSocket, Tasks.While(() => !this.isClosing));

				// If server is shutting down, skip processing this connection
				if (!clientSocket.IsCompleted)
				{
					continue;
				}

				// Assign an ID to the new client - reuse returned IDs or generate a new one
				if (!this.returnedIds.TryDequeue(out uint id))
				{
					id = nextId++;
				}

				// Add the client socket to the connections list (thread-safe)
				lock (this.connections)
				{
					this.connections.Add(clientSocket.Result);
				}

				// Notify listeners that a new client has connected
				this.queuedIds.Enqueue(new Tuple<Socket, uint>(clientSocket.Result, id));
			}
		}
	}
}