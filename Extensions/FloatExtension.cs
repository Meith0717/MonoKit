// FloatExtension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace Engine.Extensions
{
    public static class FloatExtension
    {
        public static Vector2 RadiantToDirection(this float value)
            => new(MathF.Cos(value), MathF.Sin(value));
    }
}
