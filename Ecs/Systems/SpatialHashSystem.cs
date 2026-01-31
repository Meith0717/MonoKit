// SpatialHashSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;
using MonoKit.Spatial;

namespace MonoKit.Ecs.Systems;

public class SpatialHashSystem(EcsSpatialHash grid) : ISystem
{
    public int Priority => 100;

    public void Update(double elapsedMs, ComponentManager components)
    {
        var query = components.CreateFilter().With<TransformComponent>().With<ColliderComponent>();

        foreach (var entity in query.Execute())
        {
            components.TryGetComponent<TransformComponent>(entity, out var transform);
            components.TryGetComponent<ColliderComponent>(entity, out var collider);
            grid.UpdateEntity(entity.Id, transform.Position, collider.Width, collider.Height);
        }
    }
}
