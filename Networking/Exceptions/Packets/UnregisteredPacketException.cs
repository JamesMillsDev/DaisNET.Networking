namespace DaisNET.Networking.Exceptions.Packets
{
	public class UnregisteredPacketException : Exception
	{
		public UnregisteredPacketException(ushort id, string? message)
			: base(message ?? $"Packet with id {id} is unregistered.")
		{
		}

		public UnregisteredPacketException(ushort id, string? message, Exception? innerException)
			: base(message ?? $"Packet with id {id} is unregistered.", innerException)
		{
		}
	}
}