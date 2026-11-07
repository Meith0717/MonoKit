// AppHost - Collider3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;

namespace MonoKit.GameRuntime.Ecs.Components;

public struct Collider3D(Vector3 size)
{
    public Vector3 Size = size;
}
