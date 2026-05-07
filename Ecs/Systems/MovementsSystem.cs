// MovementsSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;

namespace MonoKit.Ecs.Systems;

public class MovementsSystem : ISystem
{
    public int Priority { get; } = 0;
    private ComponentPool<Transform2D> _transform2DPool;
    private ComponentPool<Velocity2D> _velocity2DPool;

    public void Initialize(ComponentManager components)
    {
        _transform2DPool = components.GetOrCreatePool<Transform2D>();
        _velocity2DPool = components.GetOrCreatePool<Velocity2D>();
    }

    public void Update(double elapsedMs, World world)
    {
        var components = world.Components;
        var query = components.GetQuery().With<Transform2D>().With<Velocity2D>();

        query.ForEach(e =>
        {
            ref var transform = ref _transform2DPool.Get(e.Id);
            ref var velocity = ref _velocity2DPool.Get(e.Id);

            transform.Position += velocity.LinearVelocity * (float)elapsedMs;
            transform.Rotation += velocity.AngularVelocity * (float)elapsedMs;
        });
    }
}
