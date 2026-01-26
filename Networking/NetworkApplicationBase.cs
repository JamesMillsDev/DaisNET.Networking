using DaisNET.Utility;

namespace DaisNET.Networking
{
	public abstract class NetworkApplicationBase<T>(bool isServer)
		where T : NetworkPlayer, new()
	{
		public bool IsClosing { get; private set; }
		public bool IsServer { get; } = isServer;

		public async Task Run()
		{
			await Tasks.While(() => Network<T>.Instance == null);

			if (Network<T>.Instance == null)
			{
				// this should literally be impossible
				return;
			}

			RegisterPackets(Network<T>.Instance);

			this.IsClosing = false;
			Initialise(Network<T>.Instance);

			while (!ShouldClose())
			{
				Tick(Network<T>.Instance);
			}

			Shutdown(Network<T>.Instance);
			this.IsClosing = true;
		}

		protected abstract void RegisterPackets(Network<T> network);
		protected abstract bool ShouldClose();
		protected abstract void Initialise(Network<T> network);
		protected abstract void Tick(Network<T> network);
		protected abstract void Shutdown(Network<T> network);
	}
}