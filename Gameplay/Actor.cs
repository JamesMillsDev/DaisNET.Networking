using DaisNET.Networking;
using DaisNET.Networking.Packets.Gameplay;

namespace DaisNET.Gameplay
{
	public class Actor<T>(string name)
		where T : NetworkPlayer, new()
	{
		public Transform Transform { get; set; } = new();
		public string Name { get; } = name;

		public T Player { get; internal init; }

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

		internal void SyncTransform()
		{
			if (Player.IsLocalPlayer)
			{
				Network<T>.Instance?.SendPacket(
					new TransformPacket<T>(this.Name, this.Transform)
				);
			}
		}
	}
}