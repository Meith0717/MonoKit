// Velocity2D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace MonoKit.Ecs.Components;

public struct Velocity2D
{
    public float Velocity;
    public Vector2 NormalizedMovingDirection;
    public float AngularVelocity;
}
