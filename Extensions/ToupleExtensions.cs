// ToupleExtensions.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace GameEngine.Extensions
{
    public static class ToupleExtensions
    {
        public static Vector2 ToVector2(this (float, float) touple)
            => new(touple.Item1, touple.Item2);

        public static Point ToPoint(this (int, int) touple)
            => new(touple.Item1, touple.Item2);
    }
}
