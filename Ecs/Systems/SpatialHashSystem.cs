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

    public void Update(double elapsedMs, World world)
    {
        var components = world.Components;
        var query = components.CreateFilter().With<TransformComponent>().With<ColliderComponent>();

        query.ForEach(e =>
        {
            if (!components.TryGetComponent<TransformComponent>(e, out var transform))
                return;
            if (!components.TryGetComponent<ColliderComponent>(e, out var collider))
                return;

            grid.UpdateEntity(e, transform.Position, collider.Width, collider.Height);
        });
    }

    public void OnEntityDestroyed(Entity entity)
    {
        grid.RemoveEntity(entity);
    }
}
