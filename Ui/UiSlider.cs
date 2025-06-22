// UiSlider.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Engine.Ui
{
    public class UiSlider(bool hoverable) : UiElement
    {
        private float _sliderValue;
        private bool _hovered;
        private bool _pressed;
        private readonly bool _hoverable = hoverable;

        public float Value { get => _sliderValue; set => _sliderValue = value; }

        protected override void Updater(InputState inputState)
        {
            if (!_hoverable) return;
            float relativeMousePosition = inputState.MousePosition.X - Bounds.X;
            _hovered = Bounds.Contains(inputState.MousePosition);
            _pressed = _hovered && inputState.HasAction(ActionType.LeftClickHold) || _pressed;
            if (!_pressed) return;
            if (inputState.HasAction(ActionType.LeftReleased)) _pressed = false;
            if (_pressed)
                _sliderValue = float.Round(float.Clamp(relativeMousePosition / Bounds.Width, 0, 1), 1);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            Rectangle recangle = Bounds;
            spriteBatch.FillRectangle(recangle, BgColor);
            recangle.Width = (int)(recangle.Width * _sliderValue);
            spriteBatch.FillRectangle(recangle, _hovered || _pressed ? HoverColor : IdeColor);
        }

        public Color IdeColor { private get; set; } = Color.DarkOrange;
        public Color BgColor { private get; set; } = Color.DarkGray;
        public Color HoverColor { private get; set; } = Color.MonoGameOrange;
    }
}
