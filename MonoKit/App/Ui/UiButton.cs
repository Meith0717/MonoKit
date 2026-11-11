// MonoKit - UiButton.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

public struct ButtonStyle { }

public interface IButton
{
    bool Disable { get; set; }
    Action OnClickAction { get; set; }

    /// <summary>Restores the idle colors (undoes any <c>OverrideColor</c> call).</summary>
    void ResetColor();

    /// <summary>Overrides the idle colors until <c>ResetColor</c> is called.</summary>
    void OverrideColor(params Color[] color);
}

public static class UiButton
{
    private class ButtonBehaviour
    {
        public bool IsHovered;

        /// <summary>Fires the click action when the press started and was released on the button.</summary>
        public void Update(
            Rectangle bounds,
            bool mouseClicked,
            bool isDisabled,
            Action onClickAction,
            Action onHoverAction
        )
        {
            var wasHovered = IsHovered;
            IsHovered = !isDisabled && bounds.Contains(Mouse.GetState().Position);

            if (IsHovered && !wasHovered)
                onHoverAction?.Invoke();

            if (!mouseClicked || isDisabled)
                return;
            onClickAction?.Invoke();
        }
    }

    /// <summary>Button built from a texture with an optional composed label.</summary>
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
        public Action OnHoverAction { get; set; }
        public bool Disable { get; set; }
        public Action OnClickAction { get; set; }

        /// <summary>Overrides the label and texture colors until ResetColor is called.</summary>
        public void OverrideColor(params Color[] color)
        {
            if (color.Length > 0)
                UiText.Color = color.First();
            if (color.Length > 1)
                Color = color[1];
        }

        /// <summary>Restores the idle label and texture colors.</summary>
        public void ResetColor()
        {
            OverrideColor(TextIdleColor, TextureIdleColor);
        }

        /// <summary>Updates label, button behavior, and state-based colors.</summary>
        protected override void Updater(InputHandler inputHandler)
        {
            base.Updater(inputHandler);
            UiText?.Update(inputHandler, Bounds, UiScale);
            _buttonBehaviour.Update(Bounds, IsMouseClicked, Disable, OnClickAction, OnHoverAction);
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

        /// <summary>Draws the texture and its composed label.</summary>
        protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            base.Drawer(spriteBatch, graphicsDevice);
            UiText?.Draw(spriteBatch, graphicsDevice);
        }

        /// <summary>Scales the button and forwards the clip rect to the label.</summary>
        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);
            if (UiText is null)
                return;
            // Composed text is not in a container's child list, so forward the clip rect manually
            // (UiContainer.ApplyScale normally does this for added children).
            UiText.ClipRect = ClipRect;
            UiText.ApplyScale(Bounds, uiScale);
        }

        /// <summary>Disposes the button and its composed label.</summary>
        public override void Dispose()
        {
            base.Dispose();
            UiText?.Dispose();
        }
    }

    /// <summary>Button rendering just a text label.</summary>
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

        public Action OnHoverAction { get; set; }
        public bool Disable { get; set; }
        public Action OnClickAction { get; set; }

        /// <summary>Overrides the label color until ResetColor is called.</summary>
        public void OverrideColor(params Color[] color)
        {
            if (color.Length > 0)
                UiText.Color = color.First();
        }

        /// <summary>Restores the idle label color.</summary>
        public void ResetColor()
        {
            OverrideColor(TextIdleColor);
        }

        /// <summary>Updates the label and applies state-based colors.</summary>
        protected override void Updater(InputHandler inputHandler)
        {
            UiText.Update(inputHandler, Bounds, UiScale);
            _buttonBehaviour.Update(Bounds, IsMouseClicked, Disable, OnClickAction, OnHoverAction);
            UiText.Color =
                Disable ? TextDisableColor
                : _buttonBehaviour.IsHovered ? TextHoverColor
                : TextIdleColor;
        }

        /// <summary>Draws the label.</summary>
        protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            UiText.Draw(spriteBatch, graphicsDevice);
        }

        /// <summary>Scales the button and its label.</summary>
        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);
            UiText.ApplyScale(Bounds, uiScale);
        }

        /// <summary>Disposes the button and its label.</summary>
        public override void Dispose()
        {
            base.Dispose();
            UiText.Dispose();
        }
    }
}
