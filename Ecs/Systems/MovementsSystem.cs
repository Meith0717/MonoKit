// MovementsSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Threading.Tasks;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Querying;

namespace MonoKit.Ecs.Systems;

public class MovementsSystem : ISystem
{
    public int Priority { get; } = 0;
    private EntityTypeTracker _tracker;
    private ComponentPool<Transform2D> _transformPool;
    private ComponentPool<Velocity2D> _velocityPool;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _transformPool = world.Components.GetOrCreatePool<Transform2D>();
        _velocityPool = world.Components.GetOrCreatePool<Velocity2D>();
    }

    public void Update(double elapsedMs, World world)
    {
        var entities = _tracker.GetEntitiesWith<Transform2D, Velocity2D>();

        Parallel.ForEach(
            entities,
            e =>
            {
                ref var transform = ref _transformPool.Get(e.Id);
                ref var velocity = ref _velocityPool.Get(e.Id);

                transform.Position += velocity.LinearVelocity * (float)elapsedMs;
                transform.Rotation += velocity.AngularVelocity * (float)elapsedMs;
            }
        );
    }
}
