namespace DaisNET.Networking.Packets.Base
{
	/// <summary>
	/// Packet sent from server to client to establish player identity and connection information.
	/// Contains the assigned player ID, whether this is the local player, and the IP endpoint.
	/// </summary>
	public class ConnectionPacket() : Packet(PACKET_ID)
	{
		/// <summary>
		/// The unique identifier for this packet type used for registration and routing.
		/// </summary>
		public const string PACKET_ID = "connection";

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
			writer.Write(this.id);
			writer.Write(this.ipConnection);
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
			if (Network.Instance == null)
			{
				return Task.CompletedTask;
			}

			lock (Network.Instance.players)
			{
				// Create a new NetworkPlayer with the received connection information
				NetworkPlayer? player = Network.Instance.MakePlayer<NetworkPlayer>(
					this.ipConnection,
					this.id,
					this.isLocalPlayer
				);
				
				if (player == null)
				{
					throw new NullReferenceException("Player failed to construct!");
				}

				// Notify that the client has connected and add to the players list
				player.OnClientConnected();
				Network.Instance.players.Add(player);
			}

			return Task.CompletedTask;
		}
	}
}