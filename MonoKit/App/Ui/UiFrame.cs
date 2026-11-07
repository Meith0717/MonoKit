// MonoKit - UiFrame.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

public class UiFrame : UiElement
{
    private readonly List<UiElement> _children = new();
    private Color _color = Color.White;
    private Texture2D _texture;
    private float _textureScale = 1;

    public Color Color
    {
        set => _color = value;
    }

    protected override void Updater(InputHandler inputHandler)
    {
        UpdateSizeIfTextureNotNull();
        _children.RemoveAll(c => c.IsDisposed);
        foreach (var child in _children)
            child.Update(inputHandler, Bounds, UiScale);
    }

    protected override void Drawer(SpriteBatch spriteBatch)
    {
        if (_texture is not null)
            spriteBatch.Draw(_texture, Bounds, _color);
        else if (_color != Color.Transparent)
            spriteBatch.FillRectangle(Bounds, _color);

        foreach (var child in _children)
            child.Draw(spriteBatch);
    }

    public override void ApplyScale(Rectangle root, float uiScale)
    {
        base.ApplyScale(root, uiScale);
        foreach (var child in _children)
            child.ApplyScale(Bounds, uiScale);
    }

    public override void Dispose()
    {
        base.Dispose();
        foreach (var child in _children)
            child.Dispose();
    }

    public void Clear()
    {
        _children.Clear();
    }

    public void Add(UiElement child)
    {
        _children.Add(child);
    }

    public UiFrame AttachTexture(Texture2D texture, float scale = 1)
    {
        _texture = texture;
        _textureScale = scale;
        return this;
    }

    private void UpdateSizeIfTextureNotNull()
    {
        if (_texture is null)
            return;
        Width = (int)(_texture.Width * _textureScale);
        Height = (int)(_texture.Height * _textureScale);
    }
}
