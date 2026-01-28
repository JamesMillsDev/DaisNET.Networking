using DaisNET.Networking.Packets;

namespace DaisNET.Networking.Exceptions.Packets
{
	public class PacketSizeExceededException : Exception
	{
		public PacketSizeExceededException(int size)
			: base($"Packet with size {size} exceeded maximum of {Packet.MAX_PACKET_SIZE}.")
		{
			
		}
		
		public PacketSizeExceededException(int size, string? message)
			: base(message ?? $"Packet with size {size} exceeded maximum of {Packet.MAX_PACKET_SIZE}.")
		{
		}

		public PacketSizeExceededException(int size, string? message, Exception? innerException)
			: base(message ?? $"Packet with size {size} exceeded maximum of {Packet.MAX_PACKET_SIZE}.", innerException)
		{
		}
	}
}