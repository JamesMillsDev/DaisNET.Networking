namespace DaisNET.Networking.Exceptions.Packets
{
	public class PacketDeserializationException : Exception
	{
		public PacketDeserializationException(string? message)
			: base(message ?? "Packet failed to deserialize correctly.")
		{
		}

		public PacketDeserializationException(string? message, Exception? innerException)
			: base(message ?? "Packet failed to deserialize correctly.", innerException)
		{
		}
	}
}