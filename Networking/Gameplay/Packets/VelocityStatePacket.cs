using DaisNET.Networking.Packets;
using DaisNET.Networking.Serialization;

namespace DaisNET.Networking.Gameplay.Packets
{
    /// <summary>
    /// Packet that synchronizes an actor's velocity state (linear and angular velocity) across the network.
    /// Uses a generic type parameter to specify the NetworkPlayer type for type-safe network access.
    /// Identifies actors by their unique GUID rather than by name.
    /// </summary>
    public class VelocityStatePacket() : Packet
    {
       /// <summary>
       /// The globally unique identifier of the actor whose velocity state is being synchronized.
       /// </summary>
       private Guid actorId;

       /// <summary>
       /// The velocity state data (linear and angular velocity) to synchronize.
       /// </summary>
       private VelocityState velocityState = new();

       /// <summary>
       /// Initializes a new VelocityStatePacket with the specified actor ID and velocity state data.
       /// </summary>
       /// <param name="actorId">The unique identifier of the actor to update.</param>
       /// <param name="velocityState">The velocity state data to apply to the actor.</param>
       public VelocityStatePacket(Guid actorId, VelocityState velocityState) : this()
       {
          this.actorId = actorId;
          this.velocityState = velocityState;
       }

       /// <summary>
       /// Serializes the actor ID and velocity state data for network transmission.
       /// Uses a custom GuidSerializer for the GUID and the default serializer for VelocityState.
       /// </summary>
       /// <param name="writer">The PacketWriter to write data to.</param>
       public override void Serialize(PacketWriter writer)
       {
          writer.Write(this.actorId, new GuidSerializer());
          writer.Write(this.velocityState);
       }

       /// <summary>
       /// Deserializes the actor ID and velocity state data received from the network.
       /// Uses a custom GuidSerializer for the GUID and the default serializer for VelocityState.
       /// </summary>
       /// <param name="reader">The PacketReader to read data from.</param>
       public override void Deserialize(PacketReader reader)
       {
          this.actorId = reader.ReadSerialized<Guid, GuidSerializer>();
          this.velocityState = reader.ReadSerialized<VelocityState>();
       }

       /// <summary>
       /// Processes the velocity state packet.
       /// Currently validates that the network instance exists.
       /// Implementation for applying velocity state updates to actors is pending.
       /// </summary>
       /// <returns>A completed task.</returns>
       public override Task Process()
       {
          // Ensure network instance exists before processing
          if (Network.Instance == null)
          {
             // This should never happen
             return Task.CompletedTask;
          }
          
          // TODO: Find actor by actorId and apply velocityState data
          
          return Task.CompletedTask;
       }
    }
}