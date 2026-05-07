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
        var query = components.GetQuery().With<Transform2D>().With<Collider2D>();

        query.ForEach(e =>
        {
            var transform = components.GetComponent<Transform2D>(e);
            var collider = components.GetComponent<Collider2D>(e);

            grid.UpdateEntity(e, transform.Position, collider.Width, collider.Height);
        });
    }

    public void OnEntityDestroyed(Entity entity)
    {
        grid.RemoveEntity(entity);
    }
}
