// MovementsSystem2D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.Ecs.Systems;

public class MovementsSystem2D() : System<Transform2D, Velocity2D>(-99)
{
    protected override void OnInitialize(World world) { }

    protected override void ProcessEntity(
        Entity e,
        ref Transform2D transform,
        ref Velocity2D velocity,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        transform.Position +=
            velocity.Velocity * velocity.NormalizedMovingDirection * (float)elapsedMs;
        transform.Rotation += velocity.AngularVelocity * (float)elapsedMs;
    }
}
