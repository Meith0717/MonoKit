// AppHost - Velocity3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;

namespace MonoKit.Game.Components;

public struct Velocity3D(Vector3 linearVelocity, Vector3 angularVelocity)
{
    /// <summary>
    ///     Linear Velocity u
    /// </summary>
    public Vector3 LinearVelocity = linearVelocity;

    /// <summary>
    ///     Angular Velocity ω
    /// </summary>
    public Vector3 AngularVelocity = angularVelocity;
}
