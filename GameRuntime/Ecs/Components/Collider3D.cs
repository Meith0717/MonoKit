// Collider3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace MonoKit.GameRuntime.Ecs.Components;

public struct Collider3D(Vector3 size)
{
    public Vector3 Size = size;
}
