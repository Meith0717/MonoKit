// Velocity3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace MonoKit.Ecs.Components;

public struct Velocity3D(Vector3 linearVelocity, Vector3 angularVelocity)
{
    /// <summary>
    /// Linear Velocity u
    /// </summary>
    public Vector3 LinearVelocity = linearVelocity;

    /// <summary>
    /// Angular Velocity ω
    /// </summary>
    public Vector3 AngularVelocity = angularVelocity;
}
