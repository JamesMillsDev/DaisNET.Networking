using System.Net.Sockets;
using System.Text;

namespace DaisNET.Networking.Packets
{
	public static class PacketProtocols
	{
		/// <summary>
		/// Reads a complete packet from the specified socket with timeout protection.
		/// Uses a two-stage process: first reads the length prefix, then reads the packet body.
		/// Each receive operation has a 1-second timeout to prevent indefinite blocking.
		/// </summary>
		/// <param name="target">The socket to read from.</param>
		/// <returns>A tuple containing the packet ID and payload data. Returns ("NULL", empty array) if no data is available.</returns>
		/// <exception cref="TimeoutException">Thrown when the packet is not received within the timeout period (1 second per read operation).</exception>
		/// <exception cref="Exception">Thrown when the client disconnects during the read operation.</exception>
		internal static async Task<Tuple<string, byte[]>> ReadPacket(Socket target)
		{
			// Check if any data is available before attempting to read
			if (target.Available == 0)
			{
				return new Tuple<string, byte[]>("NULL", []);
			}

			// Stage 1: Read the 4-byte length prefix
			// This tells us how many bytes to expect in the packet body
			byte[] lengthBuffer = new byte[sizeof(int)];
			int totalRead = 0;
			while (totalRead < sizeof(int))
			{
				ValueTask<int> receiveValueTask = target.ReceiveAsync(lengthBuffer.AsMemory(totalRead));
				Task<int> receiveTask = receiveValueTask.AsTask();
				Task completedTask = await Task.WhenAny(receiveTask, Task.Delay(1000));

				// Check if the receive operation timed out
				if (completedTask != receiveTask)
				{
					throw new TimeoutException("Failed to read packet in time!");
				}

				int bytesRead = receiveTask.Result;

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
				ValueTask<int> receiveValueTask = target.ReceiveAsync(buffer.AsMemory(totalRead));
				Task<int> receiveTask = receiveValueTask.AsTask();
				Task completedTask = await Task.WhenAny(receiveTask, Task.Delay(1000));

				// Check if the receive operation timed out
				if (completedTask != receiveTask)
				{
					throw new TimeoutException("Failed to read packet in time!");
				}

				int bytesRead = receiveTask.Result;

				// Check if the client disconnected (0 bytes read indicates disconnection)
				if (bytesRead == 0)
				{
					throw new Exception("Client disconnected");
				}

				totalRead += bytesRead;
			}

			// Parse the packet header to extract the ID and payload
			ReadPacketHeader(buffer, out string id, out byte[] payload);
			return new Tuple<string, byte[]>(id, payload);
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
		internal static void ReadPacketHeader(byte[] buffer, out string id, out byte[] payload)
		{
			using (MemoryStream memoryStream = new(buffer))
			{
				if (!memoryStream.CanRead)
				{
					throw new InvalidOperationException("Cannot read packet header");
				}

				// Read the length of the packet ID string
				byte[] idLengthBytes = new byte[sizeof(int)];
				memoryStream.ReadExactly(idLengthBytes);

				int idLength = BitConverter.ToInt32(idLengthBytes);
				ArgumentOutOfRangeException.ThrowIfNegative(idLength);

				// Handle empty ID case (ID length is 0)
				if (idLength == 0)
				{
					id = string.Empty;
					// Everything remaining in buffer is payload
					int payloadSize = (int)(memoryStream.Length - memoryStream.Position);
					payload = new byte[payloadSize];
					memoryStream.ReadExactly(payload);

					return;
				}

				// Read the packet ID string
				byte[] idBytes = new byte[idLength];
				memoryStream.ReadExactly(idBytes);
				id = Encoding.UTF8.GetString(idBytes);

				// Read the remaining bytes as payload
				int remainingPayloadSize = (int)(memoryStream.Length - memoryStream.Position);
				payload = new byte[remainingPayloadSize];
				memoryStream.ReadExactly(payload);
			}
		}
	}
}