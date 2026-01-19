// UiSprite.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Input;

namespace MonoKit.Ui;

public class UiSprite(string texture, float scale = 1, Color color = default) : UiElement
{
    private Texture2D _texture = ContentProvider.Container<Texture2D>().Get(texture);
    public Color Color = color;
    public float Scale = scale;

    protected string Texture
    {
        set => _texture = ContentProvider.Container<Texture2D>().Get(value);
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
