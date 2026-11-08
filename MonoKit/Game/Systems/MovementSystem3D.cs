// MovementSystem3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;
using MonoKit.Game.Components;

namespace MonoKit.Game.Systems;

/// <summary>Integrates 3D velocity into transforms: position += linear * dt, orientation pre-multiplied by angular rotation * dt.</summary>
public class MovementSystem3D : IUpdateSystem
{
    /// <summary>Hook: called once when the system joins the world.</summary>
    public void Initialize(World world) { }

    /// <summary>Applies linear and angular velocity to 3D transforms.</summary>
    public void Update(double elapsedMs, World world)
    {
        var dt = (float)elapsedMs;
        world.ForEach<Transform3D, Velocity3D>((Entity _, ref Transform3D transform, ref Velocity3D velocity) =>
        {
            transform.Position += velocity.LinearVelocity * dt;
            transform.Orientation =
                Matrix.CreateFromYawPitchRoll(
                    velocity.AngularVelocity.X * dt,
                    velocity.AngularVelocity.Y * dt,
                    velocity.AngularVelocity.Z * dt
                ) * transform.Orientation;
        });
    }

    /// <summary>Releases system resources; none are held.</summary>
    public void Dispose() { }
}
