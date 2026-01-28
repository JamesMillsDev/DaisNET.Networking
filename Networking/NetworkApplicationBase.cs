using DaisNET.Networking.Gameplay;
using DaisNET.Utility;

namespace DaisNET.Networking
{
	public abstract class NetworkApplicationBase(bool isServer)
	{
		public bool IsClosing { get; private set; }
		public bool IsServer { get; } = isServer;

		public async Task Run()
		{
			await Tasks.While(() => Network.Instance == null);

			if (Network.Instance == null)
			{
				// this should literally be impossible
				return;
			}

			RegisterPackets(Network.Instance);

			this.IsClosing = false;
			Initialise(Network.Instance);

			while (!ShouldClose())
			{
				foreach ((Guid _, Actor actor) in Network.Instance.World)
				{
					actor.Tick(GetFrameTime());
				}
				
				Tick(Network.Instance);
			}

			Shutdown(Network.Instance);
			this.IsClosing = true;
		}

		protected abstract void RegisterPackets(Network network);
		protected abstract bool ShouldClose();
		protected abstract void Initialise(Network network);
		protected abstract void Tick(Network network);
		protected abstract void Shutdown(Network network);
		protected abstract float GetFrameTime();
	}
}