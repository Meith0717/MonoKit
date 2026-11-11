// MonoKit - UiText.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

/// <summary>Label rendering a single string with a sprite font.</summary>
public sealed class UiText(SpriteFont spriteFont, string text = null) : UiElement
{
    public string Text { get; set; } = text;

    public float Scale { get; set; } = 1;

    public Color Color { get; set; } = Color.Black;
    public Func<string> TextProvider { get; set; }

    /// <summary>Sizes the element to the scaled text dimensions.</summary>
    protected override void MeasureSize()
    {
        var textDimension = Text == null ? Vector2.Zero : spriteFont.MeasureString(Text);
        IntrinsicWidth = (int)(textDimension.X * Scale);
        IntrinsicHeight = (int)float.Floor(textDimension.Y * Scale);
    }

    /// <summary>Draws the text at the element bounds.</summary>
    protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        if (string.IsNullOrEmpty(Text))
            return;

        var position = Bounds.Location.ToVector2();
        spriteBatch.DrawString(
            spriteFont,
            Text,
            position,
            Color,
            0,
            Vector2.Zero,
            UiScale * Scale,
            SpriteEffects.None,
            1
        );
    }

    /// <summary>Refreshes the text from the optional provider.</summary>
    protected override void Updater(InputHandler inputHandler)
    {
        if (TextProvider != null)
            Text = TextProvider();
    }
}
