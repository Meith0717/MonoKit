// UiText.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Ui
{
    public class UiText(string spriteFont) : UiElement
    {
        private readonly SpriteFont _font = ContentProvider.Fonts.Get(spriteFont);

        private string _text;
        private bool _dirty;

        public string Text { 
            get => _text; 
            set 
            {
                _text = value;
                _dirty = true;
            }
        }

        public float Scale { private get; set; } = 1;
        public Color Color { private get; set; } = Color.Black;
        public float Alpha { private get; set; } = 1;

        public override void ApplyScale(Rectangle root, float uiScale = 1)
        {
            UpdateSize();
            base.ApplyScale(root, uiScale);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            Vector2 position = Bounds.Location.ToVector2();
            spriteBatch.DrawString(_font, Text, position, Color * Alpha, 0, Vector2.Zero, UiScale * Scale, SpriteEffects.None, 1);
        }

        protected override void Updater(InputState inputState)
        {
            if (_dirty) 
            {
                UpdateSize();
                _dirty = false;
            }
        }

        private void UpdateSize()
        {
            Vector2 textSize = _text == null ? Vector2.Zero : _font.MeasureString(_text) * Scale;
            var textDimension = textSize.ToPoint();
            Width = textDimension.X;
            Height = textDimension.Y;
        }
    }
}
