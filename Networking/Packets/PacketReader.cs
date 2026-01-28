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
	/// </summary>
	public class PacketReader
	{
		/// <summary>
		/// The memory buffer that the reader is pulling from.
		/// </summary>
		private readonly MemoryStream stream;

		public PacketReader(byte[] buffer)
		{
			if (buffer.Length >= Packet.MAX_PACKET_SIZE)
			{
				throw new PacketSizeExceededException(buffer.Length);
			}
			
			this.stream = new MemoryStream(buffer);
		}

		/// <summary>
		/// Reads a boolean value from the stream.
		/// </summary>
		/// <returns>The deserialized boolean value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public bool ReadBoolean()
		{
			return this.stream.CanRead
				? BitConverter.ToBoolean(ReadBytes(sizeof(bool)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a character from the stream.
		/// </summary>
		/// <returns>The deserialized character.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public char ReadChar()
		{
			return this.stream.CanRead
				? BitConverter.ToChar(ReadBytes(sizeof(char)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a 16-bit signed integer from the stream.
		/// </summary>
		/// <returns>The deserialized short value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public short ReadInt16()
		{
			return this.stream.CanRead
				? BitConverter.ToInt16(ReadBytes(sizeof(short)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a 32-bit signed integer from the stream.
		/// </summary>
		/// <returns>The deserialized int value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public int ReadInt32()
		{
			return this.stream.CanRead
				? BitConverter.ToInt32(ReadBytes(sizeof(int)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a 64-bit signed integer from the stream.
		/// </summary>
		/// <returns>The deserialized long value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public long ReadInt64()
		{
			return this.stream.CanRead
				? BitConverter.ToInt64(ReadBytes(sizeof(long)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a single-precision floating point number from the stream.
		/// </summary>
		/// <returns>The deserialized float value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public float ReadFloat()
		{
			return this.stream.CanRead
				? BitConverter.ToSingle(ReadBytes(sizeof(float)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a double-precision floating point number from the stream.
		/// </summary>
		/// <returns>The deserialized double value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public double ReadDouble()
		{
			return this.stream.CanRead
				? BitConverter.ToDouble(ReadBytes(sizeof(double)))
				: throw new PacketDeserializationException("Stream is not readable");
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
		/// Reads a 16-bit unsigned integer from the stream.
		/// </summary>
		/// <returns>The deserialized ushort value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public ushort ReadUInt16()
		{
			return this.stream.CanRead
				? BitConverter.ToUInt16(ReadBytes(sizeof(ushort)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a 32-bit unsigned integer from the stream.
		/// </summary>
		/// <returns>The deserialized uint value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public uint ReadUInt32()
		{
			return this.stream.CanRead
				? BitConverter.ToUInt32(ReadBytes(sizeof(uint)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a 64-bit unsigned integer from the stream.
		/// </summary>
		/// <returns>The deserialized ulong value.</returns>
		/// <exception cref="PacketDeserializationException">Thrown when the stream is not readable.</exception>
		public ulong ReadULong()
		{
			return this.stream.CanRead
				? BitConverter.ToUInt64(ReadBytes(sizeof(ulong)))
				: throw new PacketDeserializationException("Stream is not readable");
		}

		/// <summary>
		/// Reads a UTF-8 encoded string from the stream.
		/// Strings are serialized with a 4-byte length prefix followed by the UTF-8 bytes.
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
		/// Reads and deserialized an external data type using an implementation of <see cref="Serializer{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type you want to deserialize from binary data.</typeparam>
		/// <typeparam name="TSerializer">The implementation of the <see cref="Serializer{T}"/>.</typeparam>
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