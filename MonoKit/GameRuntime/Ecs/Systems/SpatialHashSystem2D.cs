// SpatialHashSystem2D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.App.Input;
using MonoKit.GameRuntime.Ecs.Components;
using MonoKit.GameRuntime.Ecs.Entities;
using MonoKit.GameRuntime.Gameplay;
using MonoKit.GameRuntime.Spatial;

namespace MonoKit.GameRuntime.Ecs.Systems;

public class SpatialHashSystem2D(EcsSpatialHash2D grid)
    : System<Transform2D, Collider2D>(-98),
        IOnEntityDestroyed
{
    protected override void OnInitialize(World world) => grid.Clear();

    protected override void ProcessEntity(
        Entity e,
        ref Transform2D transform,
        ref Collider2D collider,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        grid.UpdateEntity(e, transform.Position, collider.Width, collider.Height);
    }

    public void OnEntityDestroyed(Entity entity) => grid.RemoveEntity(entity);
}
