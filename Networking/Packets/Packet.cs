namespace DaisNET.Networking.Packets
{
	/// <summary>
	/// Abstract base class for all network packets.
	/// Packets represent discrete messages sent between client and server.
	/// Each packet has a unique ID for identification and must implement serialization, deserialization, and processing logic.
	/// </summary>
	public abstract class Packet
	{
		/// <summary>
		/// The maximum size a single packet can be. This is to prevent DOSing.
		/// </summary>
		public const int MAX_PACKET_SIZE = 1048576;
		
		/// <summary>
		/// Gets the unique identifier for this packet type.
		/// Used to determine which packet class to instantiate when receiving data.
		/// </summary>
		public ushort ID { get; internal set; }

		/// <summary>
		/// Serializes this packet's data into the provided writer.
		/// Implement this to write all packet-specific data that should be transmitted.
		/// </summary>
		/// <param name="writer">The PacketWriter to write data to.</param>
		public abstract void Serialize(PacketWriter writer);

		/// <summary>
		/// Deserializes data from the provided reader into this packet.
		/// Implement this to read all packet-specific data in the same order it was serialized.
		/// </summary>
		/// <param name="reader">The PacketReader to read data from.</param>
		public abstract void Deserialize(PacketReader reader);

		/// <summary>
		/// Processes this packet after it has been received and deserialized.
		/// Implement this to define what action should be taken when this packet is received.
		/// </summary>
		/// <returns>A task representing the asynchronous processing operation.</returns>
		public abstract Task Process();
	}
}