namespace DaisNET.Networking
{
	public class NetworkPlayer
	{
		public NetworkConnection Connection { get; internal init; }
		public bool IsLocalPlayer { get; internal set; }

		public virtual void OnClientConnected()
		{
			
		}

		public virtual void OnClientDisconnected()
		{
			
		}
	}
}