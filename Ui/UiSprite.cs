// UiSprite.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Ui
{
    public class UiSprite(string texture, float scale = 1, Color color = default) : UiElement
    {
        private Texture2D _texture = ContentProvider.Textures.Get(texture);

        protected string Texture { set => _texture = ContentProvider.Textures.Get(value); }
        protected float Scale = scale;
        protected Color Color = color;

        protected override void Updater(InputState inputState)
        {
            Width = (int)float.Ceiling(_texture.Width * Scale);
            Height = (int)float.Ceiling(_texture.Height * Scale);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_texture, Bounds, Color);
    }
}
