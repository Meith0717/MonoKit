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

    public void Update(double elapsedMs, ComponentManager components)
    {
        var query = components.CreateFilter().With<TransformComponent>().With<ColliderComponent>();

        foreach (var entity in query.Execute())
        {
            if (!components.TryGetComponent<TransformComponent>(entity, out var transform))
                continue;
            if (!components.TryGetComponent<ColliderComponent>(entity, out var collider))
                continue;

            grid.UpdateEntity(entity.Id, transform.Position, collider.Width, collider.Height);
        }
    }

    public void OnEntityDestroyed(Entity entity)
    {
        grid.RemoveEntity(entity.Id);
    }
}
