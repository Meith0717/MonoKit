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

public sealed class UiText(SpriteFont spriteFont, string text = null) : UiElement
{
    private float _scale = 1;
    private string _text = text;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateSize();
        }
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            UpdateSize();
        }
    }

    public Color Color { get; set; } = Color.Black;
    public Func<string> TextProvider { get; set; }

    public override void ApplyScale(Rectangle root, float uiScale)
    {
        UpdateSize();
        base.ApplyScale(root, uiScale);
    }

    protected override void Drawer(SpriteBatch spriteBatch)
    {
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

    private void UpdateSize()
    {
        var textDimension = _text == null ? Vector2.Zero : spriteFont.MeasureString(_text);
        Width = (int)(textDimension.X * _scale);
        Height = (int)float.Floor(textDimension.Y * _scale);
    }
}
