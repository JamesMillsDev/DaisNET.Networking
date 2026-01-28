using DaisNET.Networking.Exceptions.Packets;
using DaisNET.Networking.Packets;
using DaisNET.Networking.Serialization.Serializers;

namespace DaisNET.Networking.Gameplay.Packets
{
	public class ActorSpawnPacket() : Packet
	{
		private uint owningPlayerId;
		private Guid actorId;
		private Type? type;

		public ActorSpawnPacket(uint owningPlayerId, Guid actorId, Type type) : this()
		{
			this.owningPlayerId = owningPlayerId;
			this.actorId = actorId;
			this.type = type;
		}

		public override void Serialize(PacketWriter writer)
		{
			if (this.type == null)
			{
				throw new PacketSerializationException("ActorSpawnPacket type is null");
			}

			if (this.type.AssemblyQualifiedName == null)
			{
				throw new PacketSerializationException("ActorSpawnPacket type.AssemblyQualifiedName is null");
			}

			writer.Write(this.owningPlayerId);
			writer.Write(this.actorId, new GuidSerializer());
			writer.Write(this.type.AssemblyQualifiedName);
		}

		public override void Deserialize(PacketReader reader)
		{
			this.owningPlayerId = reader.ReadUInt32();
			this.actorId = reader.ReadSerialized<Guid, GuidSerializer>();
			string typeName = reader.ReadString();

			try
			{
				this.type = Type.GetType(typeName);
			}
			catch (Exception e)
			{
				throw new PacketDeserializationException("ActorSpawnPacket type not found", e);
			}
		}

		public override Task Process()
		{
			if (this.type == null)
			{
				throw new PacketSerializationException("ActorSpawnPacket type is null");
			}

			if (Network.Instance == null)
			{
				throw new NullReferenceException("Network instance is null");
			}

			if (Network.Instance.HasAuthority)
			{
				((NetworkServer)Network.Instance).BroadcastPacket(
					(ushort)Network.InternalPackets.SpawnActor,
					new ActorSpawnPacket(this.owningPlayerId, this.actorId, this.type)
				);
			}

			NetworkPlayer? player = Network.Instance.FindPlayer<NetworkPlayer>(this.owningPlayerId);
			if (player == null)
			{
				throw new KeyNotFoundException($"Player {this.owningPlayerId} not found");
			}

			if (!player.IsLocalPlayer)
			{
				Network.Instance.World.SpawnActorWithGuid(this.owningPlayerId, this.actorId, this.type);
			}
			return Task.CompletedTask;
		}
	}
}