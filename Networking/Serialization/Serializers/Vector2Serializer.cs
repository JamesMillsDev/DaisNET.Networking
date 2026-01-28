using System.Numerics;
using DaisNET.Utility.Extensions;

namespace DaisNET.Networking.Serialization.Serializers
{
	public class Vector2Serializer : Serializer<Vector2>
	{
		public override byte[] Serialize(Vector2 toSerialize)
		{
			List<byte> bytes = [];
			bytes.AddRange(BitConverter.GetBytes(toSerialize.X));
			bytes.AddRange(BitConverter.GetBytes(toSerialize.Y));
			return bytes.ToArray();
		}

		public override Vector2 Deserialize(byte[] data)
		{
			using (MemoryStream stream = new(data))
			{
				return new Vector2
				{
					X = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0),
					Y = BitConverter.ToSingle(stream.ReadBytes(sizeof(float)), 0)
				};
			}
		}
	}
}