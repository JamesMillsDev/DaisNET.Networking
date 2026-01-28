using System.Numerics;
using System.Runtime.InteropServices;
using DaisNET.Networking.Serialization;
using DaisNET.Networking.Serialization.Serializers;
using DaisNET.Utility.Extensions;

namespace DaisNET.Networking.Gameplay
{
    /// <summary>
    /// Represents a 3D transformation containing position, rotation, and scale.
    /// Implements IPacketSerializable to allow efficient network transmission of transform data.
    /// Uses a record type for value-based equality and immutability benefits.
    /// </summary>
    public record Transform : IPacketSerializable
    {
       /// <summary>
       /// Gets or sets the position in 3D space.
       /// Defaults to the origin (0, 0, 0).
       /// </summary>
       public Vector3 Position { get; set; } = Vector3.Zero;
       
       /// <summary>
       /// Gets or sets the rotation as a quaternion.
       /// Defaults to no rotation (identity quaternion).
       /// </summary>
       public Quaternion Rotation { get; set; } = Quaternion.Identity;
       
       /// <summary>
       /// Gets or sets the scale in 3D space.
       /// Defaults to uniform scale of 1 (1, 1, 1).
       /// </summary>
       public Vector3 Scale { get; set; } = Vector3.One;

       /// <summary>
       /// Serializer instance for converting Vector3 values to and from byte arrays.
       /// Used for both Position and Scale components.
       /// </summary>
       private readonly Serializer<Vector3> vecSerializer = new Vector3Serializer();
       
       /// <summary>
       /// Serializer instance for converting Quaternion values to and from byte arrays.
       /// Used for the Rotation component.
       /// </summary>
       private readonly Serializer<Quaternion> quatSerializer = new QuaternionSerializer();

       /// <summary>
       /// Serializes this transform into a byte array for network transmission.
       /// Concatenates the serialized position, rotation, and scale in sequence.
       /// </summary>
       /// <returns>A byte array containing the position, rotation, and scale data.</returns>
       public byte[] Serialize()
       {
          List<byte> bytes = [];

          bytes.AddRange(this.vecSerializer.Serialize(this.Position));
          bytes.AddRange(this.quatSerializer.Serialize(this.Rotation));
          bytes.AddRange(this.vecSerializer.Serialize(this.Scale));

          return bytes.ToArray();
       }

       /// <summary>
       /// Deserializes transform data from a byte array received over the network.
       /// Reads the position first, followed by rotation, then scale.
       /// </summary>
       /// <param name="data">The byte array containing the serialized transform data.</param>
       public void Deserialize(byte[] data)
       {
          using (MemoryStream stream = new(data))
          {
             this.Position = this.vecSerializer.Deserialize(stream.ReadBytes(this.vecSerializer.GetSize()));
             this.Rotation = this.quatSerializer.Deserialize(stream.ReadBytes(this.quatSerializer.GetSize()));
             this.Scale = this.vecSerializer.Deserialize(stream.ReadBytes(this.vecSerializer.GetSize()));
          }
       }

       /// <summary>
       /// Gets the total size in bytes required to serialize this transform.
       /// Equal to two Vector3 structs (position and scale) plus one Quaternion (rotation).
       /// Typically, 2 * 12 bytes + 16 bytes = 40 bytes.
       /// </summary>
       /// <returns>The size in bytes required to serialize this transform.</returns>
       public int GetSize() => Marshal.SizeOf<Vector3>() * 2 + Marshal.SizeOf<Quaternion>();
    }
}