using DaisNET.Networking;

namespace DaisNET.Gameplay
{
	public class Actor<T>(string name, T player)
		where T : NetworkPlayer, new()
	{
		public Transform Transform { get; set; } = new();
		public string Name { get; } = name;

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