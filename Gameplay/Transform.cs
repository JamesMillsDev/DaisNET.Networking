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
		public Vector2 Velocity { get; set; }

		private readonly Serializer<Vector2> serializer;

		public Transform()
		{
			this.serializer = new Vector2Serializer();
			this.Velocity = Vector2.Zero;
		}
		
		public byte[] Serialize()
		{
			List<byte> bytes = [];

			bytes.AddRange(this.serializer.Serialize(this.Position));
			bytes.AddRange(this.serializer.Serialize(this.Size));
			bytes.AddRange(this.serializer.Serialize(this.Velocity));

			return bytes.ToArray();
		}

		public void Deserialize(byte[] data)
		{
			using (MemoryStream stream = new(data))
			{
				this.Position = this.serializer.Deserialize(stream.ReadBytes(this.serializer.GetSize()));
				this.Size = this.serializer.Deserialize(stream.ReadBytes(this.serializer.GetSize()));
				this.Velocity = this.serializer.Deserialize(stream.ReadBytes(this.serializer.GetSize()));
			}
		}

		public int GetSize() => Marshal.SizeOf<Vector2>() * 3;
	}
}