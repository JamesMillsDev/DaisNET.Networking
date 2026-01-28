using System.Collections.Concurrent;
using System.Data;
using System.Net;
using System.Net.Sockets;
using DaisNET.Networking.Gameplay;
using DaisNET.Networking.Gameplay.Packets;
using DaisNET.Networking.Packets;
using DaisNET.Networking.Packets.Base;

namespace DaisNET.Networking
{
	/// <summary>
	/// Abstract base class for network implementations (server and client).
	/// Provides shared functionality for packet serialization, deserialization, and transmission.
	/// </summary>
	public abstract class Network
	{
		/// <summary>
		/// Gets the singleton instance of the current network implementation (either server or client).
		/// </summary>
		public static Network? Instance { get; private set; }

		/// <summary>
		/// Initializes and runs the network loop based on command-line arguments.
		/// Creates either a server or client instance and continuously polls for network events.
		/// </summary>
		/// <param name="app">The application instance used to check when to stop the network loop.</param>
		/// <param name="ip">The IP that the network should connect to.</param>
		/// <typeparam name="T">The type of player that will be created when <see cref="MakePlayer"/> is called.</typeparam>
		public static async Task RunNetworkLoop<T>(NetworkApplicationBase app, string ip = "")
		{
			try
			{
				await InitializeAndPoll(app, typeof(T), ip);
			}
			catch (Exception e)
			{
				Console.WriteLine($"Network - RunNetworkLoop():Exception caught: {e}");
			}
		}

		/// <summary>
		/// Creates and initializes a new network server instance.
		/// </summary>
		/// <param name="endpoint">The hostname or IP address to bind to. Defaults to "localhost".</param>
		/// <param name="port">The port number to listen on. Defaults to 25565.</param>
		private static void CreateServer(string endpoint = "localhost", int port = 25565) =>
			Instance = new NetworkServer(endpoint, port);

		/// <summary>
		/// Creates and initializes a new network client instance.
		/// </summary>
		/// <param name="endpoint">The hostname or IP address of the server to connect to.</param>
		/// <param name="port">The port number to connect to. Defaults to 25565.</param>
		private static void CreateClient(string endpoint, int port = 25565) =>
			Instance = new NetworkClient(endpoint, port);

		/// <summary>
		/// Internal method that initializes the network (server or client) and runs the polling loop.
		/// Continues polling until the application signals it is closing.
		/// </summary>
		/// <param name="app">The application instance to monitor for shutdown.</param>
		/// <param name = "type"></param>
		/// <param name="endpoint">The server endpoint address (only used for client mode).</param>
		private static async Task InitializeAndPoll(NetworkApplicationBase app, Type type, string endpoint)
		{
			if (app.IsServer)
			{
				CreateServer();
			}
			else
			{
				CreateClient(endpoint);
			}

			if (Instance == null)
			{
				throw new NullReferenceException("Network instance is null");
			}

			Instance.networkPlayerType = type;
			Instance.Open();
			Instance.RegisterDefaultPackets();

			try
			{
				// Continue polling until the application signals it's closing
				while (!app.IsClosing)
				{
					try
					{
						await Instance.Poll();
					}
					catch (Exception e)
					{
						Console.WriteLine($"Network - Initialise And Poll (Poll):Exception caught: {e}");
					}
				}

				Instance.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Network - Initialise And Poll (Initialise):Exception caught: {e}");
			}
		}

		internal enum InternalPackets : ushort
		{
			None,
			Connection,
			Transform,
			Velocity,
			SpawnActor,
			Max = 255
		}

		public ActorWorld World { get; } = new();

		/// <summary>
		/// Gets whether this network instance has authority (true for server, false for client).
		/// The authoritative instance is responsible for game state and validation.
		/// </summary>
		public bool HasAuthority { get; protected init; }
		
		/// <summary>
		/// How long the socket should attempt to receive any packets in milliseconds.
		/// default: 100ms
		/// </summary>
		public int packetReadTimeout = 100;

		/// <summary>
		/// The list of connected players which is automatically filled by <see cref="ConnectionPacket"/>.
		/// </summary>
		internal readonly List<NetworkPlayer> players = [];

		/// <summary>
		/// The underlying socket used for network communication.
		/// Null until <see cref="Open"/> is called.
		/// </summary>
		protected Socket? socket;

		/// <summary>
		/// The IP address resolved from the hostname provided in the constructor.
		/// </summary>
		protected readonly IPAddress ipAddr;

		/// <summary>
		/// The target endpoint combining the IP address and port number.
		/// </summary>
		protected readonly IPEndPoint targetEndPoint;

		/// <summary>
		/// The rate (in milliseconds) at which the <see cref="Poll"/> function will run.
		/// </summary>
		protected readonly int pollRate;

		/// <summary>
		/// Dictionary mapping packet IDs to their corresponding packet types.
		/// Used to instantiate the correct packet class when receiving data.
		/// </summary>
		private readonly ConcurrentDictionary<ushort, Type> registeredPackets = new();

		/// <summary>
		/// The type of player that will be constructed when the <see cref="MakePlayer"/> function is called.
		/// </summary>
		private Type? networkPlayerType;

		/// <summary>
		/// Initializes a new instance of the Network class with the specified hostname and port.
		/// Resolves the hostname to an IP address and creates an endpoint.
		/// </summary>
		/// <param name="hostName">The hostname to resolve (e.g., "localhost" or a domain name).</param>
		/// <param name="port">The port number to use for the connection. Defaults to 25565.</param>
		/// <param name="pollRate">How often (in milliseconds) the network should poll for changes. Defaults to 20ms.</param>
		protected Network(string hostName, int port = 25565, int pollRate = 20)
		{
			IPHostEntry ipHost = Dns.GetHostEntry(hostName);
			this.ipAddr = ipHost.AddressList[0];
			this.targetEndPoint = new IPEndPoint(this.ipAddr, port);

			this.socket = null;
			this.pollRate = pollRate;
		}

		internal T? MakePlayer<T>(string endpoint, uint id, bool isLocalPlayer) where T : NetworkPlayer
		{
			if (this.networkPlayerType == null)
			{
				throw new NullReferenceException("Network player type is null!");
			}

			if (!this.networkPlayerType.IsAssignableTo(typeof(T)))
			{
				throw new InvalidCastException($"{typeof(T).Name} is not a NetworkPlayer!");
			}

			T? player = (T?)Activator.CreateInstance(
				this.networkPlayerType,
				new NetworkConnection(IPEndPoint.Parse(endpoint))
				{
					ID = id
				},
				isLocalPlayer
			);
			return player;
		}

		/// <summary>
		/// Sends a packet through this network instance's socket.
		/// No-op if the socket is not initialized.
		/// </summary>
		/// <param name="id">The unique identifier for the packet being sent</param>
		/// <param name="packet">The packet to serialize and send.</param>
		public void SendPacket(ushort id, Packet packet)
		{
			if (this.socket == null)
			{
				return;
			}

			lock (this.socket)
			{
				PacketProtocols.SendPacket(id, packet, this.socket);
			}
		}

		/// <summary>
		/// Registers a packet type so it can be instantiated when received over the network.
		/// </summary>
		/// <param name="id">The unique ID for the packet.</param>
		/// <param name="type">The Type of the packet class.</param>
		/// <exception cref="DuplicateNameException">Thrown if a packet is attempted to be registered with a name that already exists.</exception>
		/// <returns>True if the packet was successfully registered, false if a packet with this ID already exists.</returns>
		public void RegisterPacket(ushort id, Type type)
		{
			if (id <= (ushort)InternalPackets.Max)
			{
				throw new IndexOutOfRangeException("Past id is within internal range. ID's must be 256 or greater.");
			}
			
			lock (this.registeredPackets)
			{
				if (this.registeredPackets.TryAdd(id, type))
				{
					return;
				}
			}

			throw new DuplicateNameException($"Duplicate packet id '{id}'");
		}

		/// <summary>
		/// Tries to find a player held within <see cref="players"/> with a specific ID.
		/// </summary>
		/// <param name="id">The id of the player attempting to be found.</param>
		/// <returns>A Player reference if found, null if id is not in use.</returns>
		public T? FindPlayer<T>(uint id) where T : NetworkPlayer =>
			(T?)this.players.FirstOrDefault(player => player.Connection.ID == id);

		/// <summary>
		/// Polls the network for incoming data and processes packets.
		/// Must be implemented by derived classes (NetworkServer and NetworkClient).
		/// </summary>
		/// <returns>A task representing the asynchronous poll operation.</returns>
		protected abstract Task Poll();

		/// <summary>
		/// Opens the socket and prepares it for network communication.
		/// For servers: binds to the local endpoint and starts listening.
		/// For clients: creates the socket and initiates connection to the server.
		/// </summary>
		protected abstract void Open();

		/// <summary>
		/// Gracefully shuts down and closes the socket connection.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when attempting to close a socket that is not connected.</exception>
		protected virtual void Close()
		{
			if (this.socket == null)
			{
				throw new InvalidOperationException("Socket is null");
			}

			lock (this.socket)
			{
				if (!this.socket.Connected)
				{
					return;
				}
				
				this.socket.Shutdown(SocketShutdown.Both);
				this.socket.Close();
			}
		}

		/// <summary>
		/// Attempts to create a packet instance for the given packet ID.
		/// </summary>
		/// <param name="id">The ID of the packet to create.</param>
		/// <param name="packet">Output parameter that receives the created packet instance, or null if creation failed.</param>
		/// <returns>True if a packet was successfully created, false if the ID is invalid or not registered.</returns>
		protected bool TryMakePacketFor(ushort id, out Packet? packet)
		{
			// Handle special "NULL" ID returned when no data is available
			if (id == ushort.MaxValue)
			{
				packet = null;
				return false;
			}

			lock (this.registeredPackets)
			{
				// Attempt to get the packet type from the registered packets dictionary
				if (!this.registeredPackets.TryGetValue(id, out Type? type))
				{
					Console.WriteLine($"No packet with id {id} found.");
					packet = null;
					return false;
				}

				// Create and return the packet instance
				packet = (Packet)Activator.CreateInstance(type)!;
				return true;
			}
		}

		/// <summary>
		/// Registers the default standard set of packets that are required for the system to work.
		/// </summary>
		private void RegisterDefaultPackets()
		{
			RegisterPacketInternal(InternalPackets.Connection, typeof(ConnectionPacket));
			RegisterPacketInternal(InternalPackets.Transform, typeof(TransformPacket));
			RegisterPacketInternal(InternalPackets.Velocity, typeof(VelocityStatePacket));
			RegisterPacketInternal(InternalPackets.SpawnActor, typeof(ActorSpawnPacket));
			return;

			//Special handler for default packets. This bypasses the special range
			void RegisterPacketInternal(InternalPackets id, Type type)
			{
				lock (this.registeredPackets)
				{
					if (this.registeredPackets.TryAdd((ushort)id, type))
					{
						return;
					}
				}

				throw new DuplicateNameException($"Duplicate packet id '{id}'");
			}
		}
	}
}