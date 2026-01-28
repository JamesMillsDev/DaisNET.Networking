using System.Net.Sockets;

namespace DaisNET.Networking.Packets
{
	internal record PacketPayload(ushort Id, byte[] Payload);
	
	internal static class PacketProtocols
	{
		/// <summary>
		/// Reads a complete packet from the specified socket with timeout protection.
		/// Uses a two-stage process: first reads the length prefix, then reads the packet body.
		/// Each receive operation has a 1-second timeout to prevent indefinite blocking.
		/// </summary>
		/// <param name="target">The socket to read from.</param>
		/// <param name="token">The </param>
		/// <returns>A tuple containing the packet ID and payload data. Returns ("NULL", empty array) if no data is available.</returns>
		/// <exception cref="TimeoutException">Thrown when the packet is not received within the timeout period (1 second per read operation).</exception>
		/// <exception cref="Exception">Thrown when the client disconnects during the read operation.</exception>
		internal static async Task<PacketPayload> ReadPacket(Socket target, CancellationToken token)
		{
			// Check if any data is available before attempting to read
			if (target.Available == 0)
			{
				return new PacketPayload(ushort.MaxValue, []);
			}

			// Stage 1: Read the 4-byte length prefix
			// This tells us how many bytes to expect in the packet body
			byte[] lengthBuffer = new byte[sizeof(int)];
			int totalRead = 0;
			while (totalRead < sizeof(int))
			{
				ValueTask<int> receiveValueTask = target.ReceiveAsync(lengthBuffer.AsMemory(totalRead), token);
				int bytesRead = await receiveValueTask.AsTask();

				// Check if the receive operation timed out
				if (receiveValueTask.IsCanceled)
				{
					throw new TimeoutException("Failed to read packet in time!");
				}

				// Check if the client disconnected (0 bytes read indicates disconnection)
				if (bytesRead == 0)
				{
					throw new Exception("Client disconnected");
				}

				totalRead += bytesRead;
			}

			int packetLength = BitConverter.ToInt32(lengthBuffer);

			// Stage 2: Read the packet body using the length we just received
			// The packet body contains [idLength][id][payload]
			byte[] buffer = new byte[packetLength];
			totalRead = 0;
			while (totalRead < packetLength)
			{
				ValueTask<int> receiveValueTask = target.ReceiveAsync(buffer.AsMemory(totalRead), token);
				int bytesRead = await receiveValueTask.AsTask();

				// Check if the receive operation timed out
				if (receiveValueTask.IsCanceled)
				{
					throw new TimeoutException("Failed to read packet in time!");
				}

				// Check if the client disconnected (0 bytes read indicates disconnection)
				if (bytesRead == 0)
				{
					throw new Exception("Client disconnected");
				}

				totalRead += bytesRead;
			}

			// Parse the packet header to extract the ID and payload
			ReadPacketHeader(buffer, out ushort id, out byte[] payload);
			return new PacketPayload(id, payload);
		}

		/// <summary>
		/// Serializes and sends a packet to the specified socket.
		/// The packet is sent with a 4-byte length prefix followed by the serialized packet data.
		/// Packet format: [totalLength (4 bytes)][idLength (4 bytes)][id (variable)][payload (variable)]
		/// </summary>
		/// <param name="packet">The packet to serialize and send.</param>
		/// <param name="target">The socket to send the packet to.</param>
		internal static void SendPacket(Packet packet, Socket target)
		{
			// Serialize the packet (ID + payload data)
			using (PacketWriter writer = new(packet.ID))
			{
				packet.Serialize(writer);

				byte[] serialized = writer.GetBytes();

				// Prepend the total length as a 4-byte prefix
				byte[] data = new byte[serialized.Length + sizeof(int)];
				Array.Copy(BitConverter.GetBytes(serialized.Length), data, sizeof(int));
				Array.Copy(serialized, 0, data, sizeof(int), serialized.Length);

				target.Send(data);
			}
		}
		
		/// <summary>
		/// Parses a packet buffer to extract the packet ID and payload data.
		/// Expected buffer format: [idLength (4 bytes)][id (variable)][payload (remaining)]
		/// </summary>
		/// <param name="buffer">The buffer containing the packet data.</param>
		/// <param name="id">Output parameter that receives the packet ID string.</param>
		/// <param name="payload">Output parameter that receives the packet payload bytes.</param>
		/// <exception cref="InvalidOperationException">Thrown when the buffer stream cannot be read.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the ID length is negative.</exception>
		private static void ReadPacketHeader(byte[] buffer, out ushort id, out byte[] payload)
		{
			using (MemoryStream memoryStream = new(buffer))
			{
				if (!memoryStream.CanRead)
				{
					throw new InvalidOperationException("Cannot read packet header");
				}

				// Read the length of the packet ID string
				byte[] readIdBytes = new byte[sizeof(ushort)];
				memoryStream.ReadExactly(readIdBytes);

				ushort readId = BitConverter.ToUInt16(readIdBytes);

				// Handle empty ID case (ID length is 0)
				if (readId == 0)
				{
					id = ushort.MaxValue;
					// Everything remaining in buffer is payload
					int payloadSize = (int)(memoryStream.Length - memoryStream.Position);
					payload = new byte[payloadSize];
					memoryStream.ReadExactly(payload);

					return;
				}
				
				// Read the remaining bytes as payload
				id = readId;
				int remainingPayloadSize = (int)(memoryStream.Length - memoryStream.Position);
				payload = new byte[remainingPayloadSize];
				memoryStream.ReadExactly(payload);
			}
		}
	}
}