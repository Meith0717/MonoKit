// AppHost - UiFrame.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MonoKit.App.Ui;

/// <summary>Container with a background: solid color or texture.</summary>
public class UiFrame : UiContainer
{
    private Color _color = Color.White;
    private Texture2D _texture;
    private float _textureScale = 1;

    public Color Color
    {
        get => _color;
        set => _color = value;
    }

    public UiFrame AttachTexture(Texture2D texture, float scale = 1)
    {
        _texture = texture;
        _textureScale = scale;
        return this;
    }

    protected override void DrawBackground(SpriteBatch spriteBatch)
    {
        if (_texture is not null)
            spriteBatch.Draw(_texture, Bounds, _color);
        else if (_color != Color.Transparent)
            spriteBatch.FillRectangle(Bounds, _color);
    }

    protected override void MeasureSize()
    {
        if (_texture is null)
            return;

        Width = (int)(_texture.Width * _textureScale);
        Height = (int)(_texture.Height * _textureScale);
    }
}
