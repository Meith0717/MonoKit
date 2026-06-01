// SpatialHashSystem3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;
using MonoKit.Input;
using MonoKit.Spatial;

namespace MonoKit.Ecs.Systems;

public class SpatialHashSystem3D(ISpatialGrid3D grid) : System<Transform3D, Collider3D>(-98)
{
    protected override void OnInitialize(World world) => grid.Clear();

    protected override void OnUpdateStart(double elapsedMs, World world)
    {
        grid.Clear();
    }

    protected override void ProcessEntity(
        Entity e,
        ref Transform3D transform,
        ref Collider3D collider,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        grid.Add(e, transform.Position, collider.Size);
    }
}
