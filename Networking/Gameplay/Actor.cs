namespace DaisNET.Networking.Gameplay
{
    /// <summary>
    /// Represents a networked game object (actor) with transform, physics, and lifecycle methods.
    /// Actors are owned by a NetworkPlayer and are synchronized across the network.
    /// Provides virtual methods for game logic that can be overridden by derived classes.
    /// </summary>
    public class Actor
    {
       /// <summary>
       /// Gets the transform (position, rotation, scale) of this actor.
       /// </summary>
       public Transform Transform { get; } = new();
       
       /// <summary>
       /// Gets the velocity state (linear and angular velocity) of this actor.
       /// </summary>
       public VelocityState VelocityState { get; } = new();
       
       /// <summary>
       /// Gets or sets the globally unique identifier for this actor.
       /// Used to identify this actor across the network.
       /// Can only be set internally within the assembly.
       /// </summary>
       public Guid Id { get; internal set; }
       
       /// <summary>
       /// The network player that owns this actor.
       /// </summary>
       private readonly NetworkPlayer player;

       /// <summary>
       /// Initializes a new actor owned by the specified network player.
       /// </summary>
       /// <param name="player">The network player that owns this actor.</param>
       /// <param name="id">The unique identifier for this actor.</param>
       internal Actor(NetworkPlayer player, Guid id)
       {
          this.Id = id;
          this.player = player;
       }

       /// <summary>
       /// Gets the network player that owns this actor, cast to the specified type.
       /// </summary>
       /// <typeparam name="T">The specific NetworkPlayer type to cast to.</typeparam>
       /// <returns>The owning network player cast to type T.</returns>
       public T GetNetworkPlayer<T>() where T : NetworkPlayer, new()
       {
          return (this.player as T)!;
       }

       /// <summary>
       /// Called once when the actor is first created or spawned.
       /// Override this to initialize actor-specific state and resources.
       /// </summary>
       public virtual void Start()
       {
       }

       /// <summary>
       /// Called every frame to render this actor.
       /// Override this to implement visual representation and rendering logic.
       /// </summary>
       public virtual void Render()
       {
       }

       /// <summary>
       /// Called every game tick to update this actor's logic and physics.
       /// Override this to implement game logic, movement, and behavior.
       /// </summary>
       /// <param name="dt">Delta time in seconds since the last tick.</param>
       public virtual void Tick(float dt)
       {
       }

       /// <summary>
       /// Called when the actor is being destroyed or removed from the game.
       /// Override this to clean up resources, unsubscribe from events, and perform shutdown logic.
       /// </summary>
       public virtual void Cleanup()
       {
       }
    }
}