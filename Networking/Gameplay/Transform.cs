using System.Numerics;
using System.Runtime.InteropServices;
using DaisNET.Networking.Serialization;
using DaisNET.Utility.Extensions;

namespace DaisNET.Networking.Gameplay
{
	public record Transform : IPacketSerializable
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector3 Scale { get; set; }

		private readonly Serializer<Vector3> vecSerializer;
		private readonly Serializer<Quaternion> quatSerializer;

		public Transform()
		{
			this.vecSerializer = new Vector3Serializer();
			this.quatSerializer = new QuaternionSerializer();
			
			this.Position = Vector3.Zero;
			this.Rotation = Quaternion.Identity;
			this.Scale = Vector3.One;
		}
		
		public byte[] Serialize()
		{
			List<byte> bytes = [];

			bytes.AddRange(this.vecSerializer.Serialize(this.Position));
			bytes.AddRange(this.quatSerializer.Serialize(this.Rotation));
			bytes.AddRange(this.vecSerializer.Serialize(this.Scale));

			return bytes.ToArray();
		}

		public void Deserialize(byte[] data)
		{
			using (MemoryStream stream = new(data))
			{
				this.Position = this.vecSerializer.Deserialize(stream.ReadBytes(this.vecSerializer.GetSize()));
				this.Rotation = this.quatSerializer.Deserialize(stream.ReadBytes(this.quatSerializer.GetSize()));
				this.Scale = this.vecSerializer.Deserialize(stream.ReadBytes(this.vecSerializer.GetSize()));
			}
		}

		public int GetSize() => Marshal.SizeOf<Vector3>() * 2 + Marshal.SizeOf<Quaternion>();
	}
}