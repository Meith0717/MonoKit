// UiSprite.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Ui
{
    public class UiSprite : UiElement
    {
        private Texture2D _texture;

        public Color Color { get; set; }

        public string Texture { set 
            { 
                _texture = ContentProvider.Textures.Get(value);
                Width = (int)float.Ceiling(_texture.Width);
                Height = (int)float.Ceiling(_texture.Height);
            }
        }

        public float Scale { set 
            {
                Width = (int)float.Ceiling(_texture.Width * value);
                Height = (int)float.Ceiling(_texture.Height * value);
            }
        }

        protected override void Updater(InputState inputState) {; }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Bounds, Color);
        }
    }
}
