// UiText.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Engine.Content;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Ui
{
    public class UiText(string spriteFont) : UiElement
    {
        private readonly SpriteFont _font = ContentProvider.Fonts.Get(spriteFont);

        public string Text { get; set; } = "";
        public float Scale { private get; set; } = 1;
        public Color Color { private get; set; } = Color.Black;
        public float Alpha { private get; set; } = 1;

        protected override void Updater(InputState inputState)
        {
            Vector2 textSize = Text == null ? Vector2.Zero : _font.MeasureString(Text) * Scale;
            var textDimension = textSize.ToPoint();
            Width = textDimension.X;
            Height = textDimension.Y;
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            Vector2 position = Bounds.Location.ToVector2();
            spriteBatch.DrawString(_font, Text, position, Color * Alpha, 0, Vector2.Zero, UiScale * Scale, SpriteEffects.None, 1);
        }
    }
}
