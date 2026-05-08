// SpatialHashSystem2D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Querying;
using MonoKit.Spatial;

namespace MonoKit.Ecs.Systems;

public class SpatialHashSystem2D(EcsSpatialHash2D grid) : ISystem, IOnEntityDestroyed
{
    public int Priority => 1;
    private EntityTypeTracker _tracker;
    private ComponentPool<Transform2D> _transformPool;
    private ComponentPool<Collider2D> _colliderPool;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _transformPool = world.Components.GetOrCreatePool<Transform2D>();
        _colliderPool = world.Components.GetOrCreatePool<Collider2D>();
    }

    public void Update(double elapsedMs, World world)
    {
        var entities = _tracker.GetEntitiesWith<Transform2D, Collider2D>();

        foreach (var e in entities)
        {
            ref var transform = ref _transformPool.Get(e.Id);
            ref var collider = ref _colliderPool.Get(e.Id);

            grid.UpdateEntity(e, transform.Position, collider.Width, collider.Height);
        }
    }

    public void OnEntityDestroyed(Entity entity)
    {
        grid.RemoveEntity(entity);
    }
}
