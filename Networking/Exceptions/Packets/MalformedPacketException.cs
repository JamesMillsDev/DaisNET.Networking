namespace DaisNET.Networking.Exceptions.Packets
{
	public class MalformedPacketException : Exception
	{
		public MalformedPacketException(string? message)
			: base(message ?? "Packet is malformed.")
		{
		}

		public MalformedPacketException(string? message, Exception? innerException)
			: base(message ?? "Packet is malformed.", innerException)
		{
		}
	}
}