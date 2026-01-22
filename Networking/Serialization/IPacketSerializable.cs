using DaisNET.Networking.Packets;

namespace DaisNET.Networking.Serialization
{
	/// <summary>
	/// Defines a contract for objects that can be serialized and deserialized for network transmission.
	/// Implementing types must provide custom serialization logic and specify their serialized size.
	/// </summary>
	public interface IPacketSerializable
	{
		/// <summary>
		/// Serializes this object into a byte array for network transmission.
		/// </summary>
		/// <returns>A byte array containing the serialized representation of this object.</returns>
		public byte[] Serialize();
       
		/// <summary>
		/// Deserializes data from a byte array and populates this object's fields.
		/// </summary>
		/// <param name="data">The byte array containing the serialized data.</param>
		public void Deserialize(byte[] data);
       
		/// <summary>
		/// Gets the size in bytes of this object when serialized.
		/// Used by <see cref="PacketReader"/> to determine how many bytes to read.
		/// </summary>
		/// <returns>The number of bytes this object occupies when serialized.</returns>
		public int GetSize();
	}
}