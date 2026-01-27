using DaisNET.Networking;

namespace DaisNET.Gameplay
{
	public class ActorList<T> : List<Actor<T>>
		where T : NetworkPlayer, new()
	{
		internal void NetworkSync()
		{
			foreach (Actor<T> actor in this)
			{
				actor.SyncTransform();
			}
		}
	}
}