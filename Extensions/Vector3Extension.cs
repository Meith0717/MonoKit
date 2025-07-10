// Vector3Extension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace GameEngine.Extensions
{
    public static class Vector3Extension
    {
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}
