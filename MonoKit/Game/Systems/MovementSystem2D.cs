// MonoKit - MovementSystem2D.cs
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
    /// <summary>Hook: called once when the system joins the world.</summary>
    public void Initialize(World world) { }

    /// <summary>Moves entities by velocity and rotates by angular velocity.</summary>
    public void Update(double elapsedMs, World world)
    {
        world.ForEach(
            (Entity _, ref Transform2D transform, ref Velocity2D velocity) =>
            {
                transform.Position +=
                    velocity.Velocity * velocity.NormalizedMovingDirection * (float)elapsedMs;
                transform.Rotation += velocity.AngularVelocity * (float)elapsedMs;
            }
        );
    }

    /// <summary>Releases system resources; none are held.</summary>
    public void Dispose() { }
}
