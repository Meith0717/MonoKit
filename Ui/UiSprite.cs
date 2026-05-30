// UiSprite.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Input;

namespace MonoKit.Ui;

public class UiSprite : UiElement
{
    private Texture2D _texture;
    public Color Color;
    public float Scale;

    /// <summary>
    /// Creates a UiSprite from a content name (loaded via ContentProvider).
    /// </summary>
    public UiSprite(string texture, float scale = 1, Color color = default)
    {
        _texture = ContentProvider.Container<Texture2D>().Get(texture);
        Color = color;
        Scale = scale;
    }

    /// <summary>
    /// Creates a UiSprite from an existing Texture2D instance.
    /// </summary>
    public UiSprite(Texture2D texture, float scale = 1, Color color = default)
    {
        _texture = texture;
        Color = color;
        Scale = scale;
    }

    protected string Texture
    {
        set => _texture = ContentProvider.Container<Texture2D>().Get(value);
    }

    /// <summary>
    /// Sets the texture directly (for dynamic textures not loaded via ContentProvider).
    /// </summary>
    public Texture2D SpriteTexture
    {
        get => _texture;
        set
        {
            _texture = value;
            // Update size based on new texture
            if (_texture != null)
            {
                Width = (int)float.Floor(_texture.Width * Scale);
                Height = (int)float.Floor(_texture.Height * Scale);
            }
        }
    }

    protected override void Updater(InputHandler inputHandler)
    {
        Width = (int)float.Floor(_texture.Width * Scale);
        Height = (int)float.Floor(_texture.Height * Scale);
    }

    protected override void Drawer(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, Bounds, Color);
    }
}
