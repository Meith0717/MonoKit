// AppHost - SpriteRenderer.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.Game.Components;

public struct SpriteRenderer(Texture2D texture)
{
    public Texture2D Texture = texture;
    public float TextureScale = 1;
    public Color Color = default;
    public float LayerDepth = 0;

    public int TextureWidth => Texture.Width;
    public int TextureHeight => Texture.Height;
}
