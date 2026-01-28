using System.Numerics;
using DaisNET.Utility.Extensions;

namespace DaisNET.Networking.Serialization
{
	public class Vector3Serializer : Serializer<Vector3>
	{
		public override byte[] Serialize(Vector3 toSerialize)
		{
			List<byte> bytes = [];
			bytes.AddRange(BitConverter.GetBytes(toSerialize.X));
			bytes.AddRange(BitConverter.GetBytes(toSerialize.Y));
			bytes.AddRange(BitConverter.GetBytes(toSerialize.Z));
			return bytes.ToArray();
		}

		public override Vector3 Deserialize(byte[] data)
		{
			using (MemoryStream stream = new(data))
			{
				return new Vector3
				{
					X = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0),
					Y = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0),
					Z = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0)
				};
			}
		}
	}
}