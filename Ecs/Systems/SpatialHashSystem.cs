// SpatialHashSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace MonoKit.Ecs.Systems;

public class SpatialHashSystem(EcsSpatialHash grid) : ISystem, IOnEntityDestroyed
{
    public int Priority => 1;
    private ComponentPool<Transform2D> _transform2DPool;
    private ComponentPool<Collider2D> _collider2DPool;

    public void Initialize(ComponentManager components)
    {
        _transform2DPool = components.GetOrCreatePool<Transform2D>();
        _collider2DPool = components.GetOrCreatePool<Collider2D>();
    }

    public void Update(double elapsedMs, World world)
    {
        var components = world.Components;
        var query = components.GetQuery().With<Transform2D>().With<Collider2D>();

        query.ForEach(e =>
        {
            ref var transform = ref _transform2DPool.Get(e.Id);
            ref var collider = ref _collider2DPool.Get(e.Id);

            grid.UpdateEntity(e, transform.Position, collider.Width, collider.Height);
        });
    }

    public void OnEntityDestroyed(Entity entity)
    {
        grid.RemoveEntity(entity);
    }
}
