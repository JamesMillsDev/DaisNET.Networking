namespace DaisNET.Networking.Exceptions.Packets
{
	public class PacketReadTimeoutException : Exception
	{
		public PacketReadTimeoutException(int ms, string? message)
			: base(message ?? $"Packet read timed out in {ms}ms.")
		{
		}

		public PacketReadTimeoutException(int ms, string? message, Exception? innerException)
			: base(message ?? $"Packet read timed out in {ms}ms.", innerException)
		{
		}
	}
}