using System.Net;

namespace DaisNET.Networking.Packets
{
	/// <summary>
	/// Packet sent from server to client to establish player identity and connection information.
	/// Contains the assigned player ID, whether this is the local player, and the IP endpoint.
	/// </summary>
	public class ConnectionPacket<T>() : Packet(ID_NAME)
		where T : NetworkPlayer, new()
	{
		/// <summary>
		/// The unique identifier for this packet type used for registration and routing.
		/// </summary>
		public const string ID_NAME = "connection";

		/// <summary>
		/// Indicates whether this connection packet is for the local player receiving it.
		/// </summary>
		private bool isLocalPlayer;

		/// <summary>
		/// The unique ID assigned to this player by the server.
		/// </summary>
		private uint id;

		/// <summary>
		/// The IP endpoint string for this player's connection.
		/// </summary>
		private string ipConnection = "INVALID";

		/// <summary>
		/// Initializes a new ConnectionPacket with the specified player information.
		/// </summary>
		/// <param name="isLocalPlayer">True if this packet is for the local player, false for remote players.</param>
		/// <param name="id">The unique player ID assigned by the server.</param>
		/// <param name="ipConnection">The IP endpoint string for this connection.</param>
		public ConnectionPacket(bool isLocalPlayer, uint id, string ipConnection) : this()
		{
			this.isLocalPlayer = isLocalPlayer;
			this.id = id;
			this.ipConnection = ipConnection;
		}

		/// <summary>
		/// Serializes the connection data for network transmission.
		/// </summary>
		/// <param name="writer">The PacketWriter to write data to.</param>
		public override void Serialize(PacketWriter writer)
		{
			writer.Write(this.isLocalPlayer);
			writer.Write(id);
			writer.Write(ipConnection);
		}

		/// <summary>
		/// Deserializes connection data received from the network.
		/// </summary>
		/// <param name="reader">The PacketReader to read data from.</param>
		public override void Deserialize(PacketReader reader)
		{
			this.isLocalPlayer = reader.ReadBoolean();
			this.id = reader.ReadUInt32();
			this.ipConnection = reader.ReadString();
		}

		/// <summary>
		/// Processes the connection packet by creating a NetworkPlayer and adding it to the active players list.
		/// Invokes the OnClientConnected callback to notify the application of the new player.
		/// </summary>
		/// <returns>A completed task.</returns>
		public override Task Process()
		{
			// Ensure network instance exists before processing
			if (Network<T>.Instance == null)
			{
				return Task.CompletedTask;
			}

			// Create a new NetworkPlayer with the received connection information
			T player = new()
			{
				Connection = new NetworkConnection(IPEndPoint.Parse(this.ipConnection))
				{
					ID = this.id
				},
				IsLocalPlayer = this.isLocalPlayer
			};

			// Notify that the client has connected and add to the players list
			player.OnClientConnected();
			Network<T>.Instance.players.Add(player);

			return Task.CompletedTask;
		}
	}
}