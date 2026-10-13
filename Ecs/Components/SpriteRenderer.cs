// SpriteRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.Ecs.Components;

public struct SpriteRenderer(Texture2D texture)
{
    public Texture2D Texture = texture;
    public float TextureScale = 1;
    public Color Color = default;
    public float LayerDepth = 0;

    public int TextureWidth => Texture.Width;
    public int TextureHeight => Texture.Height;
}
