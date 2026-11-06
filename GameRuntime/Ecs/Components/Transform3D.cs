// Transform3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace MonoKit.GameRuntime.Ecs.Components;

public struct Transform3D(Vector3 position, Matrix orientation, Vector3 scale)
{
    public Vector3 Position = position;
    public Matrix Orientation = orientation;
    public Vector3 Scale = scale;
}
