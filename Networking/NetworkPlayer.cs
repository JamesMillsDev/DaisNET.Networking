namespace DaisNET.Networking
{
	public class NetworkPlayer
	{
		public byte ID { get; internal set; }
		public bool IsLocalPlayer { get; internal set; }

		public virtual void OnClientConnected(byte id)
		{
			
		}
	}
}