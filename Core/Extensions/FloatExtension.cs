// FloatExtension.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;

namespace MonoKit.Core.Extensions;

public static class FloatExtension
{
    public static Vector2 RadiantToDirection(this float value)
    {
        return new Vector2(MathF.Cos(value), MathF.Sin(value));
    }
}
