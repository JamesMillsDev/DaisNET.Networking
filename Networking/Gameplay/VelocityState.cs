using System.Numerics;
using System.Runtime.InteropServices;
using DaisNET.Networking.Serialization;
using DaisNET.Networking.Serialization.Serializers;
using DaisNET.Utility.Extensions;

namespace DaisNET.Networking.Gameplay
{
    /// <summary>
    /// Represents the velocity state of a physics body, containing both linear and angular velocity.
    /// Implements IPacketSerializable to allow efficient network transmission of velocity data.
    /// Uses a record type for value-based equality and immutability benefits.
    /// </summary>
    public record VelocityState : IPacketSerializable
    {
       /// <summary>
       /// Gets or sets the linear velocity (movement in 3D space) in units per second.
       /// Represents the rate of change of position along the X, Y, and Z axes.
       /// </summary>
       public Vector3 LinearVelocity { get; set; } = Vector3.Zero;
       
       /// <summary>
       /// Gets or sets the angular velocity (rotation) in radians per second.
       /// Represents the rate of change of rotation around the X, Y, and Z axes.
       /// </summary>
       public Vector3 AngularVelocity { get; set; } = Vector3.Zero;

       /// <summary>
       /// Serializer instance used for converting Vector3 values to and from byte arrays.
       /// Shared between both velocity components for consistent serialization.
       /// </summary>
       private readonly Serializer<Vector3> serializer = new Vector3Serializer();
       
       /// <summary>
       /// Serializes this velocity state into a byte array for network transmission.
       /// Concatenates the serialized linear velocity followed by the angular velocity.
       /// </summary>
       /// <returns>A byte array containing both velocity vectors in sequence.</returns>
       public byte[] Serialize()
       {
          List<byte> bytes = [];
          
          bytes.AddRange(this.serializer.Serialize(this.LinearVelocity));
          bytes.AddRange(this.serializer.Serialize(this.AngularVelocity));
          
          return bytes.ToArray();
       }

       /// <summary>
       /// Deserializes velocity data from a byte array received over the network.
       /// Reads the linear velocity first, followed by the angular velocity.
       /// </summary>
       /// <param name="data">The byte array containing the serialized velocity data.</param>
       public void Deserialize(byte[] data)
       {
          using (MemoryStream stream = new(data))
          {
             this.LinearVelocity = this.serializer.Deserialize(stream.ReadBytes(this.serializer.GetSize()));
             this.AngularVelocity = this.serializer.Deserialize(stream.ReadBytes(this.serializer.GetSize()));
          }
       }

       /// <summary>
       /// Gets the total size in bytes required to serialize this velocity state.
       /// Equal to the size of two Vector3 structs (2 * 12 bytes = 24 bytes).
       /// </summary>
       /// <returns>The size in bytes (24 bytes for two Vector3 values).</returns>
       public int GetSize() => Marshal.SizeOf<Vector3>() * 2;
    }
}