using System.Numerics;
using DaisNET.Utility.Extensions;

namespace DaisNET.Networking.Serialization
{
	public class QuaternionSerializer : Serializer<Quaternion>
	{
		public override byte[] Serialize(Quaternion toSerialize)
		{
			List<byte> bytes = [];
			bytes.AddRange(BitConverter.GetBytes(toSerialize.X));
			bytes.AddRange(BitConverter.GetBytes(toSerialize.Y));
			bytes.AddRange(BitConverter.GetBytes(toSerialize.Z));
			bytes.AddRange(BitConverter.GetBytes(toSerialize.W));
			return bytes.ToArray();
		}

		public override Quaternion Deserialize(byte[] data)
		{
			using (MemoryStream stream = new(data))
			{
				return new Quaternion
				{
					X = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0),
					Y = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0),
					Z = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0),
					W = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0)
				};
			}
		}
	}
}