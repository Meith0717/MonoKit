// UiText.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;

namespace MonoKit.Ui;

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
