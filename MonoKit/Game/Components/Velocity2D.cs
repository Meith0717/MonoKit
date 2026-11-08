// AppHost - Velocity2D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;

namespace MonoKit.Game.Components;

public struct Velocity2D
{
    public float Velocity;
    public Vector2 NormalizedMovingDirection;
    public float AngularVelocity;
}
