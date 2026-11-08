// AppHost - UiText.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

public sealed class UiText(SpriteFont spriteFont, string text = null) : UiElement
{
    private float _scale = 1;
    private string _text = text;

    public string Text
    {
        get => _text;
        set => _text = value;
    }

    public float Scale
    {
        get => _scale;
        set => _scale = value;
    }

    public Color Color { get; set; } = Color.Black;
    public Func<string> TextProvider { get; set; }

    protected override void MeasureSize()
    {
        var textDimension = _text == null ? Vector2.Zero : spriteFont.MeasureString(_text);
        IntrinsicWidth = (int)(textDimension.X * _scale);
        IntrinsicHeight = (int)float.Floor(textDimension.Y * _scale);
    }

    protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        if (string.IsNullOrEmpty(_text))
            return;

        var position = Bounds.Location.ToVector2();
        spriteBatch.DrawString(
            spriteFont,
            _text,
            position,
            Color,
            0,
            Vector2.Zero,
            UiScale * _scale,
            SpriteEffects.None,
            1
        );
    }

    protected override void Updater(InputHandler inputHandler)
    {
        if (TextProvider != null)
            Text = TextProvider();
    }

}
