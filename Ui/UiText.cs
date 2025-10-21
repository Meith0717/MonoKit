// UiText.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using MonoKit.Content;
using MonoKit.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.Ui
{
    public sealed class UiText(string spriteFont, string text = default) : UiElement
    {
        private readonly SpriteFont _font = ContentProvider.Fonts.Get(spriteFont);
        private string _text = text;
        private float _scale = 1;

        public string Text { set { _text = value; UpdateSize(); } }
        public float Scale { set { _scale = value; UpdateSize(); } }
        public Color Color { private get; set; } = Color.Black;

        public override void ApplyScale(Rectangle root, float uiScale = 1)
        {
            UpdateSize();
            base.ApplyScale(root, uiScale);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            Vector2 position = Bounds.Location.ToVector2();

            spriteBatch.DrawString(_font, _text, position, Color, 0, Vector2.Zero, UiScale * _scale, SpriteEffects.None, 1);
        }

        protected override void Updater(InputHandler inputHandler) {; }

        private void UpdateSize()
        {
            Vector2 textDimension = _text == null ? Vector2.Zero : _font.MeasureString(_text);
            Width = (int)(textDimension.X * _scale);
            Height = (int)float.Floor(textDimension.Y * _scale);
        }
    }
}
