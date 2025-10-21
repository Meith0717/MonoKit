// UiSlider.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace GameEngine.Ui
{
    public sealed class UiSlider(bool interactive) : UiElement
    {
        private readonly ButtonStyle style = new();
        private float _sliderValue;
        private bool _hovered;
        private bool _pressed;
        private readonly bool _interactive = interactive;

        public float Value { get => _sliderValue; set => _sliderValue = float.Round(float.Clamp(value, 0, 1), 2); }

        protected override void Updater(InputHandler inputHandler)
        {
            if (!_interactive) return;
            var mousePos = Mouse.GetState().Position;
            _hovered = Bounds.Contains(mousePos);
            _pressed = _pressed || (_hovered && inputHandler.HasAction(EngineInputActions.SliderHold));
            if (_pressed)
            {
                var rectangle = Bounds;
                var ratio = rectangle.Width / rectangle.Height;
                if (ratio < 1) // Vertical
                {
                    float relativeMousePosition = Bounds.Bottom - mousePos.Y;
                    _sliderValue = relativeMousePosition / Bounds.Height;
                }
                else // Horizontal
                {
                    float relativeMousePosition = mousePos.X - Bounds.X;
                    _sliderValue = relativeMousePosition / Bounds.Width;
                }
                _sliderValue = float.Round(float.Clamp(_sliderValue, 0, 1), 2);
                _pressed = !inputHandler.HasAction(EngineInputActions.ButtonPressed);
            }
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            var rectangle = Bounds;
            spriteBatch.FillRectangle(rectangle, BgColor);

            var ratio = rectangle.Height == 0 ? 0 : rectangle.Width / rectangle.Height;
            if (ratio < 1)
            {
                var bottom = rectangle.Bottom;
                rectangle.Height = (int)(rectangle.Height * _sliderValue);
                rectangle.Y = bottom - rectangle.Height;
            }
            else
                rectangle.Width = (int)(rectangle.Width * _sliderValue);

            spriteBatch.FillRectangle(rectangle, _hovered || _pressed ? HoverColor : IdeColor);
        }

        public Color BgColor { private get; set; } = new(20, 20, 20);
        public Color IdeColor { private get; set; } = Color.MonoGameOrange;
        public Color HoverColor { private get; set; } = Color.Orange;
    }
}
