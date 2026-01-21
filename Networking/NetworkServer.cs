using System.Net.Sockets;
using DaisNET.Networking.Packets;
using DaisNET.Utility;

namespace DaisNET.Networking
{
	public delegate void ClientConnectionDelegate(byte id);

	// TODO: Work out client's disconnecting
	public class NetworkServer : Network
	{
		private static byte nextId;

		public event ClientConnectionDelegate? OnClientConnected;
		public event ClientConnectionDelegate? OnClientDisconnected;

		public int connectionBacklogCount = 10;

		private readonly List<Socket> connections = [];
		private bool isClosing;

		private readonly Queue<byte> returnedIds = [];

		/// <summary>
		/// Initializes a new network server instance and starts the connection acceptance loop.
		/// Sets <see cref="Network.HasAuthority"/> to true as the server has authoritative control.
		/// </summary>
		/// <param name="host">The hostname or IP address to bind the server to.</param>
		/// <param name="port">The port number to listen on. Defaults to 25565.</param>
		public NetworkServer(string host, int port = 25565) : base(host, port)
		{
			// Server has authoritative control over game state
			this.HasAuthority = true;

			// Start the connection acceptance loop in the background
			_ = Task.Run(async () =>
			{
				try
				{
					await AwaitConnections();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
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
			this.socket.Listen(this.connectionBacklogCount);
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
				if (!this.returnedIds.TryDequeue(out byte id))
				{
					id = nextId++;
				}

				// Add the client socket to the connections list (thread-safe)
				lock (this.connections)
				{
					this.connections.Add(clientSocket.Result);
				}

				// Notify listeners that a new client has connected
				this.OnClientConnected?.Invoke(id);
				Console.WriteLine("Client: {0} connected - Assigning id: {1}", clientSocket.Result.RemoteEndPoint, id);
			}
		}
	}
}