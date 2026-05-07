// Collider3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace MonoKit.Ecs.Components;

public struct Collider3D
{
    public Vector3 Size;

    public Collider3D(Vector3 size)
    {
        Size = size;
    }
}
