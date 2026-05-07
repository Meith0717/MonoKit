// SpatialHashSystem3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Querying;
using MonoKit.Spatial;

namespace MonoKit.Ecs.Systems;

public class SpatialHashSystem3D(EcsSpatialHash3D grid) : ISystem, IOnEntityDestroyed
{
    public int Priority => 1;
    public EcsSpatialHash3D Grid { get; } = grid;
    private readonly EcsSpatialHash3D _grid = grid;

    private EntityTypeTracker _tracker;
    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<Collider3D> _colliderPool;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _transformPool = world.Components.GetOrCreatePool<Transform3D>();
        _colliderPool = world.Components.GetOrCreatePool<Collider3D>();
    }

    public void Update(double elapsedMs, World world)
    {
        var entities = _tracker.GetEntitiesWith<Transform3D, Collider3D>();

        foreach (var e in entities)
        {
            ref var transform = ref _transformPool.Get(e.Id);
            ref var collider = ref _colliderPool.Get(e.Id);

            _grid.UpdateEntity(e, transform.Position, collider.Size);
        }
    }

    public void OnEntityDestroyed(Entity entity)
    {
        _grid.RemoveEntity(entity);
    }
}
