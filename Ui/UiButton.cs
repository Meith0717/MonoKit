// UiButton.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Content;
using MonoKit.Input;

namespace MonoKit.Ui;

public struct ButtonStyle() { }

public interface IButton
{
    bool Disable { get; set; }
    Action OnClickAction { get; set; }
    void OverrideColor(params Color[] color);
}

public static class UiButton
{
    private class ButtonBehaviour()
    {
        private bool _isPressed;
        private bool _wasHovered;
        public bool IsHovered;

        public void Update(
            InputHandler inputHandler,
            Rectangle bounds,
            Action onClickAction,
            Action onHoverAction,
            bool isDisabled
        )
        {
            IsHovered = !isDisabled && bounds.Contains(Mouse.GetState().Position);
            _isPressed = IsHovered && inputHandler.HasAction(EngineInputActions.ButtonPressed);
            _wasHovered = IsHovered;

            if (IsHovered && !_wasHovered)
                onHoverAction?.Invoke();

            if (!_isPressed)
                return;
            onClickAction?.Invoke();
        }
    }

    public class Sprite(Texture2D texture2D) : UiSprite(texture2D), IButton
    {
        private readonly ButtonBehaviour _buttonBehaviour = new();

        public Color TextIdleColor { get; set; } = Color.White;
        public Color TextHoverColor { get; set; } = Color.MonoGameOrange;
        public Color TextDisableColor { get; set; } = Color.DarkGray;
        public Color TextureIdleColor { get; set; } = Color.White;
        public Color TextureHoverColor { get; set; } = Color.MonoGameOrange;
        public Color TextureDisableColor { get; set; } = Color.DarkGray;
        public UiText UiText { get; init; }
        public bool Disable { get; set; }
        public Action OnClickAction { get; set; }
        public Action OnHoverAction { get; set; }

        public void OverrideColor(params Color[] color)
        {
            if (color.Length > 0)
                UiText.Color = color.First();
            if (color.Length > 1)
                Color = color[1];
        }

        protected override void Updater(InputHandler inputHandler)
        {
            base.Updater(inputHandler);
            UiText?.Update(inputHandler, Bounds, UiScale);
            _buttonBehaviour.Update(inputHandler, Bounds, OnClickAction, OnHoverAction, Disable);
            Color =
                Disable ? TextureDisableColor
                : _buttonBehaviour.IsHovered ? TextureHoverColor
                : TextureIdleColor;
            if (UiText is not null)
                UiText.Color =
                    Disable ? TextDisableColor
                    : _buttonBehaviour.IsHovered ? TextHoverColor
                    : TextIdleColor;
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            base.Drawer(spriteBatch);
            UiText?.Draw(spriteBatch);
        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);
            UiText?.ApplyScale(Bounds, uiScale);
        }

        public override void Dispose()
        {
            base.Dispose();
            UiText?.Dispose();
        }
    }

    public class Text(ButtonStyle? style = null) : UiElement, IButton
    {
        private readonly ButtonBehaviour _buttonBehaviour = new();
        public Color TextIdleColor { get; set; } = Color.White;
        public Color TextHoverColor { get; set; } = Color.MonoGameOrange;
        public Color TextDisableColor { get; set; } = Color.DarkGray;
        public UiText UiText { get; init; }
        public float Scale
        {
            set => UiText.Scale = value;
        }
        public bool Disable { get; set; }
        public Action OnClickAction { get; set; }
        public Action OnHoverAction { get; set; }

        public void OverrideColor(params Color[] color)
        {
            if (color.Length > 0)
                UiText.Color = color.First();
        }

        protected override void Updater(InputHandler inputHandler)
        {
            UiText.Update(inputHandler, Bounds, UiScale);
            _buttonBehaviour.Update(inputHandler, Bounds, OnClickAction, OnHoverAction, Disable);
            UiText.Color =
                Disable ? TextDisableColor
                : _buttonBehaviour.IsHovered ? TextHoverColor
                : TextIdleColor;
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            UiText.Draw(spriteBatch);
        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);
            UiText.ApplyScale(Bounds, uiScale);
        }

        public override void Dispose()
        {
            base.Dispose();
            UiText.Dispose();
        }
    }
}
