using System.Collections;
using DaisNET.Networking.Gameplay.Packets;

namespace DaisNET.Networking.Gameplay
{
	public class ActorWorld : IEnumerable<KeyValuePair<Guid, Actor>>
	{
		private readonly Dictionary<Guid, Actor> actors = [];

		public T Spawn<T>(uint owningPlayerId) where T : Actor
		{
			if (Network.Instance == null)
			{
				throw new NullReferenceException("Network instance is null");
			}

			NetworkPlayer? player = Network.Instance.FindPlayer<NetworkPlayer>(owningPlayerId);
			if (player == null)
			{
				throw new NullReferenceException($"Player {owningPlayerId} not found");
			}

			Guid actorId = Guid.NewGuid();

			if (Activator.CreateInstance(typeof(T), player, actorId) is not T actor)
			{
				throw new NullReferenceException($"Actor of type {typeof(T).Name} failed to construct.");
			}

			player.OnOwnedActorSpawned(actor);
			actors.Add(actorId, actor);
			Network.Instance.SendPacket(
				(ushort)Network.InternalPackets.SpawnActor,
				new ActorSpawnPacket(owningPlayerId, actorId, typeof(T))
			);

			return actor;
		}

		public T? FindActor<T>(Guid id) where T : Actor
		{
			if (this.actors.TryGetValue(id, out Actor? actor))
			{
				return (T?)actor;
			}

			throw new KeyNotFoundException($"Actor with ID {id} not found");
		}

		internal void SpawnActorWithGuid(uint owningPlayerId, Guid actorId, Type type)
		{
			if (Network.Instance == null)
			{
				throw new NullReferenceException("Network instance is null");
			}

			NetworkPlayer? player = Network.Instance.FindPlayer<NetworkPlayer>(owningPlayerId);
			if (player == null)
			{
				throw new NullReferenceException($"Player {owningPlayerId} not found");
			}

			if (Activator.CreateInstance(type, player, actorId) is not Actor actor)
			{
				throw new NullReferenceException($"Actor of type {type.Name} failed to construct.");
			}

			player.OnOwnedActorSpawned(actor);
			actors.Add(actorId, actor);
		}

		public IEnumerator<KeyValuePair<Guid, Actor>> GetEnumerator() => this.actors.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.actors.GetEnumerator();
	}
}