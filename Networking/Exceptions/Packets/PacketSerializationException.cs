namespace DaisNET.Networking.Exceptions.Packets
{
	public class PacketSerializationException : Exception
	{
		public PacketSerializationException(string? message)
			: base(message ?? "Packet failed to serialize correctly.")
		{
		}

		public PacketSerializationException(string? message, Exception? innerException)
			: base(message ?? "Packet failed to serialize correctly.", innerException)
		{
		}
	}
}