using DaisNET.Gameplay;

namespace DaisNET.Networking.Packets.Gameplay
{
	/// <summary>
	/// Packet that synchronizes an actor's transform (position, rotation, scale) across the network.
	/// Used to keep game objects in sync between server and clients.
	/// </summary>
	public class TransformPacket<T>() : Packet(ID_NAME)
		where T : NetworkPlayer, new()
	{
		/// <summary>
		/// The unique identifier for this packet type used for registration and routing.
		/// </summary>
		public const string ID_NAME = "transform";

		/// <summary>
		/// The name of the actor whose transform is being synchronized.
		/// </summary>
		private string actorName = "";

		/// <summary>
		/// The transform data (position, rotation, scale) to synchronize.
		/// </summary>
		private Transform transform = new();

		/// <summary>
		/// Initializes a new TransformPacket with the specified actor and transform data.
		/// </summary>
		/// <param name="actorName">The name of the actor to update.</param>
		/// <param name="transform">The transform data to apply to the actor.</param>
		public TransformPacket(string actorName, Transform transform) : this()
		{
			this.actorName = actorName;
			this.transform = transform;
		}

		/// <summary>
		/// Serializes the actor name and transform data for network transmission.
		/// </summary>
		/// <param name="writer">The PacketWriter to write data to.</param>
		public override void Serialize(PacketWriter writer)
		{
			writer.Write(this.actorName);
			writer.Write(this.transform);
		}

		/// <summary>
		/// Deserializes the actor name and transform data received from the network.
		/// </summary>
		/// <param name="reader">The PacketReader to read data from.</param>
		public override void Deserialize(PacketReader reader)
		{
			this.actorName = reader.ReadString();
			this.transform = reader.ReadPacketSerializable<Transform>();
		}

		/// <summary>
		/// Processes the transform packet.
		/// If this is the server (has authority), broadcasts the transform update to all clients.
		/// If this is a client, updates the corresponding actor's transform (currently commented out).
		/// </summary>
		/// <returns>A completed task.</returns>
		public override Task Process()
		{
			// Ensure network instance exists before processing
			if (Network<T>.Instance == null)
			{
				// This should never happen
				return Task.CompletedTask;
			}

			// Server: Broadcast the transform update to all clients
			if (Network<T>.Instance.HasAuthority)
			{
				((NetworkServer<T>)Network<T>.Instance).BroadcastPacket(
					new TransformPacket<T>(this.actorName, this.transform)
				);
			}

			return Task.CompletedTask;
		}
	}
}