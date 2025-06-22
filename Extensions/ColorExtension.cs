// ColorExtension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace Engine.Extensions
{
    public static class ColorExtension
    {
        public static Color Tints(this Color color, float factor)
        {
            Vector3 white3 = Color.White.ToVector3();
            Vector3 color3 = color.ToVector3();
            Vector3 thinColor = color3 + (white3 - color3) * factor;
            return new Color(thinColor.X, thinColor.Y, thinColor.Z);
        }

        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            hue = hue % 360;
            float c = value * saturation;
            float x = c * (1 - MathF.Abs((hue / 60f) % 2 - 1));
            float m = value - c;

            float r = 0, g = 0, b = 0;

            if (hue < 60) { r = c; g = x; }
            else if (hue < 120) { r = x; g = c; }
            else if (hue < 180) { g = c; b = x; }
            else if (hue < 240) { g = x; b = c; }
            else if (hue < 300) { r = x; b = c; }
            else { r = c; b = x; }

            return new Color(r + m, g + m, b + m);
        }

        public static Color FloatToColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f); // Clamp to [0, 1]
            float hue = t * 360f;            // Map [0, 1] to hue [0, 360]
            return ColorFromHSV(hue, 1f, 1f);
        }
    }
}
