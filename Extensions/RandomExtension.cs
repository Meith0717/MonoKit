// RandomExtension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace Engine.Extensions
{
    public static class RandomExtension
    {
        public static T NextFromList<T>(this Random random, List<T> lst)
            => lst[random.Next(lst.Count)];

        public static T NextFromArray<T>(this Random random, T[] lst)
            => lst[random.Next(lst.Length)];

        public static Vector2 NextVectorInCircle(this Random random, CircleF circle)
        {
            circle.Radius = random.Next(0, (int)circle.Radius);
            return circle.BoundaryPointAt(random.NextAngle());
        }

        public static Vector2 NextVectorOnBorder(this Random random, CircleF circle)
            => circle.BoundaryPointAt(random.NextAngle());
    }
}
