using System.Numerics;
using System.Runtime.InteropServices;
using DaisNET.Networking.Serialization;
using DaisNET.Utility.Extensions;

namespace DaisNET.Gameplay
{
	public record Transform : IPacketSerializable
	{
		public Vector2 Position { get; set; }
		public Vector2 Size { get; set; }

		private readonly Serializer<Vector2> serializer;

		public Transform()
		{
			this.serializer = new Vector2Serializer();
		}
		
		public byte[] Serialize()
		{
			List<byte> bytes = [];

			bytes.AddRange(serializer.Serialize(Position));
			bytes.AddRange(serializer.Serialize(Size));

			return bytes.ToArray();
		}

		public void Deserialize(byte[] data)
		{
			using (MemoryStream stream = new(data))
			{
				Position = this.serializer.Deserialize(stream.ReadBytes(this.serializer.GetSize()));
				Size = this.serializer.Deserialize(stream.ReadBytes(this.serializer.GetSize()));
			}
		}

		public int GetSize() => Marshal.SizeOf<Vector2>() * 2;
	}
}