// LoadingCircle.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace GameEngine
{
    public static class LoadingCircle
    {
        private static Vector2[] CreateCircle(double radius, int sides)
        {
            var array = new Vector2[sides];
            double num = double.Pi * 2 / sides;
            double num2 = -double.Pi / 2;
            for (int i = 0; i < sides; i++)
            {
                array[i] = new Vector2((float)(radius * double.Cos(num2)), (float)(radius * double.Sin(num2)));
                num2 += num;
            }

            return array;
        }

        public static void DrawLoadingCircle(this SpriteBatch spriteBatch, Vector2 position, float radius, double percentage, float thickness)
        {
            const int Sides = 100;
            var circle = CreateCircle(radius, Sides);
            percentage -= double.Truncate(percentage);

            if (percentage < 0)
            {
                Array.Reverse(circle);
                percentage = -percentage;
            }

            var value = (int)double.Ceiling(Sides * percentage);
            for (var i = 0; i < value; i++)
            {
                var p1 = position + circle[i];
                var p2 = position + circle[(i + 1) % Sides];

                spriteBatch.DrawLine(p1, p2, Color.White, thickness);
            }
        }
    }
}
