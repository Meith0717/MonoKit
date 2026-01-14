// Vector2Extension.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using Microsoft.Xna.Framework;

namespace MonoKit.Core.Extensions;

public static class Vector2Extension
{
    public static float AngleTo(this Vector2 vector, Vector2 vector1)
    {
        var directionVector = vector1 - vector;
        var rotation = directionVector.InclinationAngle();
        if (directionVector.Y < 0)
            rotation = 2f * MathF.PI - MathF.Abs(rotation);
        return float.IsNaN(rotation) ? 0 : rotation;
    }

    public static Vector2 RotateVector(this Vector2 vector, float radiant)
    {
        var cosTheta = MathF.Cos(radiant);
        var sinTheta = MathF.Sin(radiant);
        var x = vector.X * cosTheta - vector.Y * sinTheta;
        var y = vector.X * sinTheta + vector.Y * cosTheta;
        return new Vector2(x, y);
    }

    public static Vector2 DirectionToVector2(this Vector2 vector, Vector2 target)
    {
        if (Vector2.DistanceSquared(vector, target) == 0)
            return Vector2.One;
        return Vector2.Normalize(target - vector);
    }

    public static Vector2 Transform(this Vector2 vector, Matrix matrix)
    {
        return Vector2.Transform(vector, matrix);
    }

    public static Vector2 RotateVectorAround(this Vector2 vector, Vector2 center, float radiant)
    {
        return vector.RotateVector(radiant) + center;
    }

    public static float InclinationAngle(this Vector2 vector)
    {
        if (vector == Vector2.Zero)
            return 0;
        return MathF.Atan2(vector.Y, vector.X);
    }

    public static Vector2 InDirection(this Vector2 vector, Vector2 dir, float length)
    {
        return vector + Vector2.Normalize(dir) * length;
    }
}
