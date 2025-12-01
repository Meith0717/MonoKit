// UiButton.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Content;
using MonoKit.Input;

namespace MonoKit.Ui
{
    public struct ButtonStyle()
    {
        public Color TextIdleColor = Color.White;
        public Color TextHoverColor = Color.MonoGameOrange;
        public Color TextDisableColor = Color.DarkGray;
        public Color TextureIdleColor = Color.White;
        public Color TextureHoverColor = Color.MonoGameOrange;
        public Color TextureDisableColor = Color.DarkGray;
    }

    public interface IButton
    {
        Action OnClickAction { get; set; }
        void OveideColor(params Color[] color);
    }

    public static class UiButton
    {
        public class Sprite(string texture, ButtonStyle? style = null) : UiSprite(texture), IButton
        {
            private readonly ButtonStyle _style = style == null ? new() : style.Value;
            private readonly ButtonBehaviour _buttonBehaviour = new();
            private UiText _uiText;

            public Action OnClickAction { get; set; }
            public UiText UiText { set => _uiText = value; }
            public bool Disable;

            public void OveideColor(params Color[] color)
            {
                if (color.Length > 0)
                    _uiText.Color = color.First();
                if (color.Length > 1)
                    Color = color[1];
            }

            protected override void Updater(InputHandler inputHandler)
            {
                base.Updater(inputHandler);
                _uiText?.Update(inputHandler, Bounds, UiScale);
                _buttonBehaviour.Update(inputHandler, Bounds, OnClickAction, Disable);
                Color = Disable ? _style.TextureDisableColor : _buttonBehaviour.IsHovered ? _style.TextureHoverColor : _style.TextureIdleColor;
                if (_uiText is not null)
                    _uiText.Color = Disable ? _style.TextDisableColor : _buttonBehaviour.IsHovered ? _style.TextHoverColor : _style.TextIdleColor;
            }

            protected override void Drawer(SpriteBatch spriteBatch)
            {
                base.Drawer(spriteBatch);
                _uiText?.Draw(spriteBatch);
            }

            public override void ApplyScale(Rectangle root, float uiScale)
            {
                base.ApplyScale(root, uiScale);
                _uiText?.ApplyScale(Bounds, uiScale);
            }

            public override void Dispose()
            {
                base.Dispose();
                _uiText?.Dispose();
            }
        }

        public class Text(ButtonStyle? style = null) : UiElement, IButton
        {
            private readonly ButtonStyle _style = style == null ? new() : style.Value;
            private readonly ButtonBehaviour _buttonBehaviour = new();
            private UiText _uiText;

            public UiText UiText { set => _uiText = value; }
            public float Scale { set => _uiText.Scale = value; }
            public Action OnClickAction { get; set; }
            public bool Disable;

            protected override void Updater(InputHandler inputHandler)
            {
                _uiText.Update(inputHandler, Bounds, UiScale);
                _buttonBehaviour.Update(inputHandler, Bounds, OnClickAction, Disable);
                _uiText.Color = Disable ? _style.TextDisableColor : _buttonBehaviour.IsHovered ? _style.TextHoverColor : _style.TextIdleColor;
            }

            public void OveideColor(params Color[] color)
            {
                if (color.Length > 0)
                    _uiText.Color = color.First();
            }

            protected override void Drawer(SpriteBatch spriteBatch)
            {
                _uiText.Draw(spriteBatch);
            }

            public override void ApplyScale(Rectangle root, float uiScale)
            {
                base.ApplyScale(root, uiScale);
                _uiText.ApplyScale(Bounds, uiScale);
            }

            public override void Dispose()
            {
                base.Dispose();
                _uiText.Dispose();
            }
        }
    }

    internal class ButtonBehaviour
    {
        private bool _wasHovered;
        private bool _isPressed;
        public bool IsHovered;

        public void Update(InputHandler inputHandler, Rectangle bounds, Action onClickAction, bool isDisabled)
        {
            IsHovered = !isDisabled && bounds.Contains(Mouse.GetState().Position);
            _isPressed = IsHovered && inputHandler.HasAction(EngineInputActions.ButtonPressed);
            _wasHovered = IsHovered;

            if (IsHovered && !_wasHovered)
                AudioService.SFX.PlaySound("hoverButton");

            if (!_isPressed) return;
            AudioService.SFX.PlaySound("clickButton");
            onClickAction?.Invoke();
        }
    }
}
