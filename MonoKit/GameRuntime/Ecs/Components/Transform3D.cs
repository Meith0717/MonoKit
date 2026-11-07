// MonoKit - Transform3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace MonoKit.GameRuntime.Ecs.Components;

public struct Transform3D(Vector3 position, Matrix orientation, Vector3 scale)
{
    public Vector3 Position = position;
    public Matrix Orientation = orientation;
    public Vector3 Scale = scale;
}
