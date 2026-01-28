namespace DaisNET.Networking.Serialization.Serializers
{
	public class GuidSerializer : Serializer<Guid>
	{
		public override byte[] Serialize(Guid toSerialize)
		{
			return toSerialize.ToByteArray();
		}

		public override Guid Deserialize(byte[] data)
		{
			return new Guid(data);
		}
	}
}