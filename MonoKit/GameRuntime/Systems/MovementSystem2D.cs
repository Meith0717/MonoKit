// MovementSystem2D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.GameRuntime.ArchetypeEcs;
using MonoKit.GameRuntime.ArchetypeEcs.Entities;
using MonoKit.GameRuntime.ArchetypeEcs.Systems;
using MonoKit.GameRuntime.Components;

namespace MonoKit.GameRuntime.Systems;

/// <summary>Integrates velocity into transforms: position += velocity * direction * dt, rotation += angularVelocity * dt.</summary>
public class MovementSystem2D : IUpdateSystem
{
    public void Initialize(World world) { }

    public void Update(double elapsedMs, World world)
    {
        world.ForEach<Transform2D, Velocity2D>((Entity _, ref Transform2D transform, ref Velocity2D velocity) =>
        {
            transform.Position +=
                velocity.Velocity * velocity.NormalizedMovingDirection * (float)elapsedMs;
            transform.Rotation += velocity.AngularVelocity * (float)elapsedMs;
        });
    }

    public void Dispose() { }
}
