using System.Buffers.Binary;
using System.Text;
using DaisNET.Networking.Serialization;

namespace DaisNET.Networking.Packets
{
	/// <summary>
	/// Provides methods for serializing data into a byte stream for network transmission.
	/// Used to construct packet payloads that will be sent over the network.
	/// Automatically writes the packet ID on construction and supports various primitive types and custom serializable objects.
	/// All numeric types are serialized in little-endian byte order for consistency across platforms.
	/// </summary>
	public class PacketWriter : IDisposable, IAsyncDisposable
	{
		/// <summary>
		/// Serializes an object into a byte array based on its type.
		/// Supports primitive types (int, bool, float, etc.), strings, and custom <see cref="IPacketSerializable"/> objects.
		/// Strings are serialized with a 4-byte length prefix followed by UTF-8 encoded bytes.
		/// All numeric types are serialized in little-endian format.
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
				return SerializeInt32((int)data);
			}

			if (type == typeof(double))
			{
				return SerializeDouble((double)data);
			}

			if (type == typeof(bool))
			{
				return SerializeBoolean((bool)data);
			}

			if (type == typeof(char))
			{
				return SerializeChar((char)data);
			}

			if (type == typeof(short))
			{
				return SerializeInt16((short)data);
			}

			if (type == typeof(long))
			{
				return SerializeInt64((long)data);
			}

			if (type == typeof(float))
			{
				return SerializeSingle((float)data);
			}

			if (type == typeof(ushort))
			{
				return SerializeUInt16((ushort)data);
			}

			if (type == typeof(uint))
			{
				return SerializeUInt32((uint)data);
			}

			if (type == typeof(ulong))
			{
				return SerializeUInt64((ulong)data);
			}

			if (type == typeof(byte))
			{
				return [(byte)data];
			}

			if (type == typeof(string))
			{
				return SerializeString((string)data);
			}

			return typeof(IPacketSerializable).IsAssignableFrom(type)
				? ((IPacketSerializable)data).Serialize()
				: throw new NotSupportedException($"Type {type} is not supported for serialization.");
		}

		/// <summary>
		/// Serializes a boolean value as a single byte (1 for true, 0 for false).
		/// </summary>
		/// <param name="value">The boolean value to serialize.</param>
		/// <returns>A single-byte array containing 1 or 0.</returns>
		private static byte[] SerializeBoolean(bool value)
		{
			return [value ? (byte)1 : (byte)0];
		}

		/// <summary>
		/// Serializes a 16-bit signed integer in little-endian format.
		/// </summary>
		/// <param name="value">The short value to serialize.</param>
		/// <returns>A 2-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeInt16(short value)
		{
			byte[] buffer = new byte[sizeof(short)];
			BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a 16-bit unsigned integer in little-endian format.
		/// </summary>
		/// <param name="value">The ushort value to serialize.</param>
		/// <returns>A 2-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeUInt16(ushort value)
		{
			byte[] buffer = new byte[sizeof(ushort)];
			BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a 32-bit signed integer in little-endian format.
		/// </summary>
		/// <param name="value">The int value to serialize.</param>
		/// <returns>A 4-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeInt32(int value)
		{
			byte[] buffer = new byte[sizeof(int)];
			BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a 32-bit unsigned integer in little-endian format.
		/// </summary>
		/// <param name="value">The uint value to serialize.</param>
		/// <returns>A 4-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeUInt32(uint value)
		{
			byte[] buffer = new byte[sizeof(uint)];
			BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a 64-bit signed integer in little-endian format.
		/// </summary>
		/// <param name="value">The long value to serialize.</param>
		/// <returns>An 8-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeInt64(long value)
		{
			byte[] buffer = new byte[sizeof(long)];
			BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a 64-bit unsigned integer in little-endian format.
		/// </summary>
		/// <param name="value">The ulong value to serialize.</param>
		/// <returns>An 8-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeUInt64(ulong value)
		{
			byte[] buffer = new byte[sizeof(ulong)];
			BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a single-precision floating point number in little-endian format.
		/// </summary>
		/// <param name="value">The float value to serialize.</param>
		/// <returns>A 4-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeSingle(float value)
		{
			byte[] buffer = new byte[sizeof(float)];
			BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a double-precision floating point number in little-endian format.
		/// </summary>
		/// <param name="value">The double value to serialize.</param>
		/// <returns>An 8-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeDouble(double value)
		{
			byte[] buffer = new byte[sizeof(double)];
			BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a character as a 16-bit unsigned integer in little-endian format.
		/// </summary>
		/// <param name="value">The char value to serialize.</param>
		/// <returns>A 2-byte array containing the little-endian representation.</returns>
		private static byte[] SerializeChar(char value)
		{
			byte[] buffer = new byte[sizeof(char)];
			BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
			return buffer;
		}

		/// <summary>
		/// Serializes a string with a 4-byte little-endian length prefix followed by UTF-8 encoded bytes.
		/// </summary>
		/// <param name="value">The string value to serialize.</param>
		/// <returns>A byte array containing the length prefix and UTF-8 string data.</returns>
		private static byte[] SerializeString(string value)
		{
			byte[] stringBytes = Encoding.UTF8.GetBytes(value);
			byte[] buffer = new byte[sizeof(int) + stringBytes.Length];

			// Write length prefix in little-endian
			BinaryPrimitives.WriteInt32LittleEndian(buffer, stringBytes.Length);

			// Copy string bytes
			Array.Copy(stringBytes, 0, buffer, sizeof(int), stringBytes.Length);

			return buffer;
		}

		/// <summary>
		/// The memory stream that accumulates the serialized packet data.
		/// </summary>
		private readonly MemoryStream stream;

		/// <summary>
		/// Initializes a new PacketWriter and automatically writes the packet ID to the stream.
		/// Sets the stream capacity to the maximum packet size.
		/// </summary>
		/// <param name="id">The unique identifier for this packet type.</param>
		public PacketWriter(ushort id)
		{
			this.stream = new MemoryStream();
			this.stream.Capacity = Packet.MAX_PACKET_SIZE;
			Write(id);
		}

		/// <summary>
		/// Writes serialized data to the internal stream using the default serialization logic.
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
		/// Writes data to the internal stream using a custom serializer implementation.
		/// Allows for type-specific serialization logic beyond the default primitive type handling.
		/// No-op if the stream is not writable.
		/// </summary>
		/// <typeparam name="T">The type of data to serialize. Must have a parameterless constructor.</typeparam>
		/// <param name="data">The data to serialize into the memory stream.</param>
		/// <param name="serializer">The custom serializer implementation for the passed object.</param>
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
		/// Disposes of the internal memory stream and releases resources.
		/// </summary>
		public void Dispose()
		{
			this.stream.Dispose();
		}

		/// <summary>
		/// Asynchronously disposes of the internal memory stream and releases resources.
		/// </summary>
		public async ValueTask DisposeAsync()
		{
			await this.stream.DisposeAsync();
		}
	}
}