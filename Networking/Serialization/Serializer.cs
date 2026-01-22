using System.Runtime.InteropServices;

namespace DaisNET.Networking.Serialization
{
	/// <summary>
	/// A contract that allows the binary serialization of types that cannot have <see cref="IPacketSerializable"/>
	/// implemented. i.e., <see cref="System.Numerics.Vector2"/>.
	/// </summary>
	/// <typeparam name="T">The data type this contract is responsible for handling.</typeparam>
	public abstract class Serializer<T> where T : new()
	{
		/// <summary>
		/// Attempts to serialize the passed value into binary data.
		/// </summary>
		/// <param name="toSerialize">The data to be serialized into binary.</param>
		public abstract byte[] Serialize(T toSerialize);
		
		/// <summary>
		/// Attempts to deserialize a binary array and returns an instance of T.
		/// </summary>
		/// <param name="data">The binary data attempting to be deserialized.</param>
		public abstract T Deserialize(byte[] data);
		
		/// <summary>
		/// Returns the memory size of the type to be serialized.
		/// </summary>
		public int GetSize() => Marshal.SizeOf<T>();
	}
}