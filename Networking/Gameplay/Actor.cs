namespace DaisNET.Networking.Gameplay
{
	public class Actor
	{
		public Transform Transform { get; } = new();
		public Guid Id { get; internal set; }
		private readonly NetworkPlayer player;

		internal Actor(NetworkPlayer player, Guid id)
		{
			this.Id = id;
			this.player = player;
		}

		public T GetNetworkPlayer<T>() where T : NetworkPlayer, new()
		{
			return (this.player as T)!;
		}

		public virtual void Start()
		{
		}

		public virtual void Render()
		{
		}

		public virtual void Tick(float dt)
		{
		}

		public virtual void Cleanup()
		{
		}
	}
}