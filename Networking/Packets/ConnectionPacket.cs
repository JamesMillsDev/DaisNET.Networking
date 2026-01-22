namespace DaisNET.Networking.Packets
{
	public class ConnectionPacket() : Packet(ID_NAME)
	{
		public const string ID_NAME = "connection";

		private bool isLocalPlayer;
		private byte id;
		private string ip = "INVALID";

		public ConnectionPacket(bool isLocalPlayer, byte id, string ip) : this()
		{
			this.isLocalPlayer = isLocalPlayer;
			this.id = id;
			this.ip = ip;
		}

		public override void Serialize(PacketWriter writer)
		{
			writer.Write(this.isLocalPlayer);
			writer.Write(id);
			writer.Write(ip);
		}

		public override void Deserialize(PacketReader reader)
		{
			this.isLocalPlayer = reader.ReadBoolean();
			this.id = reader.ReadByte();
			this.ip = reader.ReadString();
		}

		public override Task Process()
		{
			if (Network.Instance == null)
			{
				return Task.CompletedTask;
			}

			NetworkPlayer player = new()
			{
				ID = this.id,
				IsLocalPlayer = this.isLocalPlayer
			};
				
			player.OnClientConnected(this.id);
			Network.Instance.players.Add(player);
			
			return Task.CompletedTask;
		}
	}
}