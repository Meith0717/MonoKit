// MonoKit - SpatialHashSystem3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.App.Input;
using MonoKit.GameRuntime.Ecs.Components;
using MonoKit.GameRuntime.Ecs.Entities;
using MonoKit.GameRuntime.Gameplay;
using MonoKit.GameRuntime.Spatial;

namespace MonoKit.GameRuntime.Ecs.Systems;

public class SpatialHashSystem3D(EcsSpatialHash3D grid) : System<Transform3D, Collider3D>(-98)
{
    protected override void OnInitialize(World world)
    {
        grid.Clear();
    }

    protected override void OnUpdateStart(double elapsedMs, World world)
    {
        grid.Clear();
    }

    protected override void ProcessEntity(
        Entity e,
        ref Transform3D transform,
        ref Collider3D collider3D,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        grid.Add(e, transform.Position, collider3D.Size);
    }
}
