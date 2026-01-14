// FloatExtension.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

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
