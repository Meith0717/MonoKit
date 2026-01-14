// ISpatial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoKit.Spatial;

public interface ISpatial
{
    Vector2 Position { get; }
    RectangleF Bounding { get; }
    bool HasPositionChanged();
}
