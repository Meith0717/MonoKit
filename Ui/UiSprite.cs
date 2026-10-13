// UiSprite.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;

namespace MonoKit.Ui;

public class UiSprite(Texture2D texture2D, float scale = 1, Color color = default) : UiElement
{
    public Color Color = color;
    public float Scale = scale;

    public Texture2D Texture2D { get; set; } = texture2D;

    protected override void Updater(InputHandler inputHandler)
    {
        if (Texture2D == null)
        {
            Width = 1;
            Height = 1;
            return;
        }

        Width = (int)float.Floor(Texture2D.Width * Scale);
        Height = (int)float.Floor(Texture2D.Height * Scale);
    }

    protected override void Drawer(SpriteBatch spriteBatch)
    {
        if (Texture2D == null)
            return;
        spriteBatch.Draw(Texture2D, Bounds, Color);
    }
}
