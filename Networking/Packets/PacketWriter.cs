using System.Text;
using DaisNET.Networking.Serialization;

namespace DaisNET.Networking.Packets
{
	/// <summary>
	/// Provides methods for serializing data into a byte stream for network transmission.
	/// Used to construct packet payloads that will be sent over the network.
	/// Automatically writes the packet ID on construction and supports various primitive types and custom serializable objects.
	/// </summary>
	public class PacketWriter : IDisposable, IAsyncDisposable
	{
		/// <summary>
		/// Serializes an object into a byte array based on its type.
		/// Supports primitive types (int, bool, float, etc.), strings, and custom <see cref="IPacketSerializable"/> objects.
		/// Strings are serialized with a 4-byte length prefix followed by UTF-8 encoded bytes.
		/// </summary>
		/// <param name="data">The object to serialize. Cannot be null.</param>
		/// <returns>A byte array containing the serialized data.</returns>
		/// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
		/// <exception cref="NotSupportedException">Thrown when the data type is not supported for serialization.</exception>
		private static byte[] Serialize(object? data)
		{
			ArgumentNullException.ThrowIfNull(data);

			Type type = data.GetType();

			if (type == typeof(int))
			{
				return BitConverter.GetBytes((int)data);
			}

			if (type == typeof(double))
			{
				return BitConverter.GetBytes((double)data);
			}

			if (type == typeof(bool))
			{
				return BitConverter.GetBytes((bool)data);
			}

			if (type == typeof(char))
			{
				return BitConverter.GetBytes((char)data);
			}

			if (type == typeof(short))
			{
				return BitConverter.GetBytes((short)data);
			}

			if (type == typeof(long))
			{
				return BitConverter.GetBytes((long)data);
			}

			if (type == typeof(float))
			{
				return BitConverter.GetBytes((float)data);
			}

			if (type == typeof(ushort))
			{
				return BitConverter.GetBytes((ushort)data);
			}

			if (type == typeof(uint))
			{
				return BitConverter.GetBytes((uint)data);
			}

			if (type == typeof(ulong))
			{
				return BitConverter.GetBytes((ulong)data);
			}

			if (type == typeof(byte))
			{
				return [(byte)data];
			}

			if (type != typeof(string))
			{
				return typeof(IPacketSerializable).IsAssignableFrom(type)
					? ((IPacketSerializable)data).Serialize()
					: throw new NotSupportedException($"Type {type} is not supported.");
			}

			// Serialize string with length prefix
			byte[] bytes = Encoding.UTF8.GetBytes((string)data);
			byte[] serialized = new byte[bytes.Length + sizeof(int)];
			Array.Copy(BitConverter.GetBytes(bytes.Length), serialized, sizeof(int));
			Array.Copy(bytes, 0, serialized, sizeof(int), bytes.Length);

			return serialized;
		}

		/// <summary>
		/// The memory buffer that the writer is streaming into.
		/// </summary>
		private readonly MemoryStream stream;

		/// <summary>
		/// Initializes a new PacketWriter and automatically writes the packet ID to the stream.
		/// </summary>
		/// <param name="id">The unique identifier for this packet type.</param>
		public PacketWriter(ushort id)
		{
			this.stream = new MemoryStream();
			this.stream.Capacity = Packet.MAX_PACKET_SIZE;
			Write(id);
		}

		/// <summary>
		/// Writes serialized data to the internal stream.
		/// No-op if the stream is not writable.
		/// </summary>
		/// <param name="data">The object to serialize and write.</param>
		public void Write(object data)
		{
			if (!this.stream.CanWrite)
			{
				return;
			}

			this.stream.Write(Serialize(data));
		}

		/// <summary>
		/// Writes the passed data into the internal stream using an implementation of <see cref="Serializer{T}"/>.
		/// </summary>
		/// <param name="data">The data to serialize into the memory stream.</param>
		/// <param name="serializer">The serializer implementation for the passed object.</param>
		public void Write<T>(T data, Serializer<T> serializer) where T : new()
		{
			if (!this.stream.CanWrite)
			{
				return;
			}

			this.stream.Write(serializer.Serialize(data));
		}

		/// <summary>
		/// Gets all bytes written to the stream as a byte array.
		/// This represents the complete serialized packet payload.
		/// </summary>
		/// <returns>A byte array containing all data written to this writer.</returns>
		public byte[] GetBytes() => this.stream.ToArray();

		/// <summary>
		/// Disposes of the internal memory stream and suppresses finalization.
		/// </summary>
		public void Dispose()
		{
			this.stream.Dispose();
		}

		/// <summary>
		/// Asynchronously disposes of the internal memory stream and suppresses finalization.
		/// </summary>
		public async ValueTask DisposeAsync()
		{
			await this.stream.DisposeAsync();
		}
	}
}