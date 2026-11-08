// MovementSystem2D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;
using MonoKit.Game.Components;

namespace MonoKit.Game.Systems;

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
