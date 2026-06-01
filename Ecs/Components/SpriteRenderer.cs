// SpriteRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace MonoKit.Ecs.Components;

public struct SpriteRenderer()
{
    public int TextureId = 0;
    public int TextureWidth = 0;
    public int TextureHeight = 0;
    public float TextureScale = 1;
    public Color Color = default;
    public float LayerDepth = 0;
}
