// EllypseFExtension.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoKit.Core.Extensions;

public static class EllipseFFExtension
{
    public static Vector2 BoundaryPointAt(this EllipseF ellipseF, float angleRadians)
    {
        var x = (float)(ellipseF.Center.X + ellipseF.RadiusX * double.Cos(angleRadians));
        var y = (float)(ellipseF.Center.Y + ellipseF.RadiusY * double.Sin(angleRadians));
        return new Vector2(x, y);
    }
}
