using DaisNET.Networking;

namespace DaisNET.Gameplay
{
	public class Actor<T>(T player)
		where T : NetworkPlayer, new()
	{
		public Transform Transform { get; set; } = new();
		public Guid Id { get; internal init; } = Guid.Empty;
		public T Player { get; } = player;

		public virtual void Init()
		{
		}

		public virtual void Render()
		{
		}

		public virtual void Tick(float dt)
		{
		}

		public virtual void DeInit()
		{
		}
	}
}