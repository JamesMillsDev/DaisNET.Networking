using System.Net.Sockets;
using DaisNET.Networking.Networking.Packets;
using DaisNET.Networking.Utility;

namespace DaisNET.Networking.Networking
{
	/// <summary>
	/// Client-side network implementation that connects to a remote server.
	/// Manages connection to a single server socket and handles incoming packet processing.
	/// </summary>
	public class NetworkClient(string endpoint, int port) : Network(endpoint, port)
	{
		/// <summary>
		/// Whether the client is connected to any server.
		/// </summary>
		public bool Connected { get; private set; }

		/// <summary>
		/// How long the client should attempt to connect to the server in milliseconds.
		/// default: 5000ms
		/// </summary>
		public int connectionTimeout = 5000;

		/// <summary>
		/// Continuously attempts to handle packets that are received from the server.
		/// </summary>
		protected override async Task Poll()
		{
			// Skip polling if not connected to server or socket is not initialized
			if (!this.Connected || this.socket == null)
			{
				return;
			}

			// Read a complete packet from the server socket
			Task<Tuple<string, byte[]>> reading = ReadPacket(this.socket);
			await reading;

			string id = reading.Result.Item1;

			// Attempt to instantiate the appropriate packet type based on the received ID
			if (TryMakePacketFor(id, out Packet? packet))
			{
				// Deserialize the packet payload and process it
				byte[] payload = reading.Result.Item2;
				PacketReader reader = new(payload);

				packet!.Deserialize(reader);
				await packet.Process();
			}

			// Wait for the configured poll interval before checking for more packets
			await Task.Delay(this.pollRate);
		}

		/// <summary>
		/// Creates the socket and initiates connection to the server.
		/// </summary>
		protected override void Open()
		{
			try
			{
				this.socket = new Socket(this.ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				_ = Task.Run(async () =>
				{
					// Attempt to connect to the server specified
					Task<bool> task = AwaitServerConnection();
					await task;

					// Store the result of the connection attempt.
					this.Connected = task.Result;
				});
			}
			catch (ArgumentNullException ane)
			{
				Console.WriteLine("ArgumentNullException : {0}", ane);
				throw;
			}
			catch (SocketException se)
			{
				Console.WriteLine("SocketException : {0}", se);
				throw;
			}
			catch (Exception e)
			{
				Console.WriteLine("Unexpected exception : {0}", e);
				throw;
			}
		}

		/// <summary>
		/// Attempts to connect to the <see cref="Network.targetEndPoint"/>. Will time out after
		/// <see cref="connectionTimeout"/> milliseconds.
		/// </summary>
		/// <returns> Whether the client connected successfully. </returns>
		private async Task<bool> AwaitServerConnection()
		{
			await Tasks.While(() => this.socket == null);

			Task connectionTask = this.socket!.ConnectAsync(this.targetEndPoint);
			Task completedTask = await Task.WhenAny(connectionTask, Task.Delay(this.connectionTimeout));

			return completedTask == connectionTask;
		}
	}
}