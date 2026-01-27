namespace DaisNET.Networking
{
	public class NetworkPlayer(NetworkConnection connection, bool isLocalPlayer)
	{
		public NetworkConnection Connection { get; } = connection;
		public bool IsLocalPlayer { get; internal set; } = isLocalPlayer;

		public virtual void OnClientConnected()
		{
			
		}

		public virtual void OnClientDisconnected()
		{
			
		}
	}
}