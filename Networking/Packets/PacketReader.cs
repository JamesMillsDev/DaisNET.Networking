using System.Buffers.Binary;
using System.Text;
using DaisNET.Networking.Exceptions.Packets;
using DaisNET.Networking.Serialization;
using DaisNET.Utility.Extensions;

namespace DaisNET.Networking.Packets
{
	/// <summary>
	/// Provides methods for reading serialized data from a byte buffer.
	/// Used to deserialize packet payloads received over the network.
	/// All read operations check if the stream is readable before attempting to read.
	/// All numeric types are deserialized using little-endian byte order for consistency across platforms.
	/// </summary>
	public class PacketReader
	{
		/// <summary>
		/// The memory stream containing the packet data to be read.
		/// </summary>
		private readonly MemoryStream stream;

		/// <summary>
		/// Initializes a new PacketReader with the provided byte buffer.
		/// Validates that the buffer does not exceed the maximum packet size.
		/// </summary>
		/// <param name="buffer">The byte buffer containing the serialized packet data.</param>
		/// <exception cref="PacketSizeExceededException">Thrown when the buffer size exceeds the maximum allowed packet size.</exception>
		public PacketReader(byte[] buffer)
		{
			if (buffer.Length >= Packet.MAX_PACKET_SIZE)
			{
				throw new PacketSizeExceededException(buffer.Length);
			}
			
			this.stream = new MemoryStream(buffer);
		}

		/// <summary>
		/// Reads a boolean value from the stream (1 byte: 1 for true, 0 for false).
		/// </summary>
		/// <returns>The deserialized boolean value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public bool ReadBoolean()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			byte value = ReadBytes(sizeof(bool))[0];
			return value != 0;
		}

		/// <summary>
		/// Reads a character from the stream as a 16-bit unsigned integer in little-endian format.
		/// </summary>
		/// <returns>The deserialized character.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public char ReadChar()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return (char)BinaryPrimitives.ReadUInt16LittleEndian(ReadBytes(sizeof(char)));
		}

		/// <summary>
		/// Reads a 16-bit signed integer from the stream in little-endian format.
		/// </summary>
		/// <returns>The deserialized short value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public short ReadInt16()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return BinaryPrimitives.ReadInt16LittleEndian(ReadBytes(sizeof(short)));
		}

		/// <summary>
		/// Reads a 32-bit signed integer from the stream in little-endian format.
		/// </summary>
		/// <returns>The deserialized int value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public int ReadInt32()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return BinaryPrimitives.ReadInt32LittleEndian(ReadBytes(sizeof(int)));
		}

		/// <summary>
		/// Reads a 64-bit signed integer from the stream in little-endian format.
		/// </summary>
		/// <returns>The deserialized long value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public long ReadInt64()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return BinaryPrimitives.ReadInt64LittleEndian(ReadBytes(sizeof(long)));
		}

		/// <summary>
		/// Reads a single-precision floating point number from the stream in little-endian format.
		/// </summary>
		/// <returns>The deserialized float value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public float ReadFloat()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return BinaryPrimitives.ReadSingleLittleEndian(ReadBytes(sizeof(float)));
		}

		/// <summary>
		/// Reads a double-precision floating point number from the stream in little-endian format.
		/// </summary>
		/// <returns>The deserialized double value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public double ReadDouble()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return BinaryPrimitives.ReadDoubleLittleEndian(ReadBytes(sizeof(double)));
		}

		/// <summary>
		/// Reads a single byte from the stream.
		/// </summary>
		/// <returns>The deserialized byte value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public byte ReadByte()
		{
			return this.stream.CanRead
				? ReadBytes(sizeof(byte))[0]
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a 16-bit unsigned integer from the stream in little-endian format.
		/// </summary>
		/// <returns>The deserialized ushort value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public ushort ReadUInt16()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return BinaryPrimitives.ReadUInt16LittleEndian(ReadBytes(sizeof(ushort)));
		}

		/// <summary>
		/// Reads a 32-bit unsigned integer from the stream in little-endian format.
		/// </summary>
		/// <returns>The deserialized uint value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public uint ReadUInt32()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(sizeof(uint)));
		}

		/// <summary>
		/// Reads a 64-bit unsigned integer from the stream in little-endian format.
		/// </summary>
		/// <returns>The deserialized ulong value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public ulong ReadULong()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			return BinaryPrimitives.ReadUInt64LittleEndian(ReadBytes(sizeof(ulong)));
		}

		/// <summary>
		/// Reads a UTF-8 encoded string from the stream.
		/// Strings are serialized with a 4-byte little-endian length prefix followed by the UTF-8 bytes.
		/// </summary>
		/// <returns>The deserialized string, or an empty string if length is 0.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable or the string length is negative.</exception>
		public string ReadString()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			int length = ReadInt32();

			if (length < 0)
			{
				throw new PacketDeserializationException("Invalid string length");
			}

			return length == 0
				? string.Empty
				: Encoding.UTF8.GetString(ReadBytes(length));
		}

		/// <summary>
		/// Reads and deserializes a custom packet-serializable object from the stream.
		/// The object must implement <see cref="IPacketSerializable"/> and have a parameterless constructor.
		/// </summary>
		/// <typeparam name="T">The type of object to deserialize, must implement <see cref="IPacketSerializable"/>.</typeparam>
		/// <returns>The deserialized object instance.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public T ReadSerialized<T>() where T : IPacketSerializable, new()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			T packetSerializable = new();
			packetSerializable.Deserialize(ReadBytes(packetSerializable.GetSize()));

			return packetSerializable;
		}

		/// <summary>
		/// Reads and deserializes an object using a custom serializer implementation.
		/// Allows for type-specific deserialization logic for types that don't implement <see cref="IPacketSerializable"/>.
		/// </summary>
		/// <typeparam name="T">The type to deserialize. Must have a parameterless constructor.</typeparam>
		/// <typeparam name="TSerializer">The custom serializer type that handles deserialization for type T.</typeparam>
		/// <returns>The deserialized object instance.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public T ReadSerialized<T, TSerializer>() where T : new() where TSerializer : Serializer<T>, new()
		{
			if (!this.stream.CanRead)
			{
				throw new PacketDeserializationException("Stream is not readable");
			}

			TSerializer serializer = new();
			return serializer.Deserialize(ReadBytes(serializer.GetSize()));
		}

		/// <summary>
		/// Reads the specified number of bytes from the internal stream.
		/// </summary>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>A byte array containing the read data.</returns>
		private byte[] ReadBytes(int count) => this.stream.ReadBytes(count);
	}
}