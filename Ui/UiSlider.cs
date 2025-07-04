// UiSlider.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace GameEngine.Ui
{
    public class UiSlider(bool interactive) : UiElement
    {
        private float _sliderValue;
        private bool _hovered;
        private bool _pressed;
        private readonly bool _interactive = interactive;

        public float Value { get => _sliderValue; set => _sliderValue = float.Round(float.Clamp(value, 0, 1), 2); }

        protected override void Updater(InputState inputState)
        {
            if (!_interactive) return;
            _hovered = Bounds.Contains(inputState.MousePosition);
            _pressed = _pressed || (_hovered && inputState.HasAction(ActionType.LeftClickHold));
            if (_pressed)
            {
                var rectangle = Bounds;
                var ratio = rectangle.Width / rectangle.Height;
                if (ratio < 1) // Vertical
                {
                    float relativeMousePosition = Bounds.Bottom - inputState.MousePosition.Y;
                    _sliderValue = relativeMousePosition / Bounds.Height;
                }
                else // Horizontal
                {
                    float relativeMousePosition = inputState.MousePosition.X - Bounds.X;
                    _sliderValue = relativeMousePosition / Bounds.Width;
                }
                _sliderValue = float.Round(float.Clamp(_sliderValue, 0, 1), 2);
                _pressed = !inputState.HasAction(ActionType.LeftReleased); 
            }
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            var rectangle = Bounds;
            spriteBatch.FillRectangle(rectangle, BgColor);

            var ratio = rectangle.Width / rectangle.Height;
            if (ratio < 1) // Vertical
            { 
                var bottom = rectangle.Bottom;
                rectangle.Height = (int)(rectangle.Height * _sliderValue);
                rectangle.Y = bottom - rectangle.Height;
            }
            else // Horizontal
            { 
                rectangle.Width = (int)(rectangle.Width * _sliderValue); 
            }

            spriteBatch.FillRectangle(rectangle, _hovered || _pressed ? HoverColor : IdeColor);
        }

        public Color IdeColor { private get; set; } = Color.DarkOrange;
        public Color BgColor { private get; set; } = Color.DarkGray;
        public Color HoverColor { private get; set; } = Color.MonoGameOrange;
    }
}
