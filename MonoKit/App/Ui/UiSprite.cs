// AppHost - UiSprite.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

/// <summary>Image element sized from its texture.</summary>
public class UiSprite(Texture2D texture2D, float scale = 1, Color color = default) : UiElement
{
    public Color Color = color;
    public float Scale = scale;

    public Texture2D Texture2D { get; set; } = texture2D;

    /// <summary>Sizes the element to the scaled texture dimensions.</summary>
    protected override void MeasureSize()
    {
        if (Texture2D == null)
        {
            IntrinsicWidth = 1;
            IntrinsicHeight = 1;
            return;
        }

        IntrinsicWidth = (int)float.Floor(Texture2D.Width * Scale);
        IntrinsicHeight = (int)float.Floor(Texture2D.Height * Scale);
    }

    /// <summary>Hook: sprites need no per-frame input handling.</summary>
    protected override void Updater(InputHandler inputHandler) { }

    /// <summary>Draws the texture into the element bounds.</summary>
    protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        if (Texture2D == null)
            return;
        spriteBatch.Draw(Texture2D, Bounds, Color);
    }
}
