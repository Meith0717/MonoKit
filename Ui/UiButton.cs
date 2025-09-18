// UiButton.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngine.Ui
{
    public struct ButtonStyle()
    {
        public Color TextIdleColor = Color.White;
        public Color TextHoverColor = Color.Gray;
        public Color TextDisableColor = Color.DarkGray;
        public Color TextureIdleColor = Color.White;
        public Color TextureHoverColor = Color.MonoGameOrange;
        public Color TextureDisableColor = Color.DarkGray;
    }

    public class UiButton(string texture) : UiSprite(texture)
    {
        private readonly ButtonStyle _style = new();
        private UiText _uiText;
        private bool _wasHovered;

        public UiText Text { set => _uiText = value; }
        public Action OnClickAction;
        public bool Disable;

        protected override void Updater(InputState inputState)
        {
            base.Updater(inputState);
            _uiText?.Update(inputState, Bounds, UiScale);

            var isDisabled = OnClickAction is null || Disable;
            var isHovered = !isDisabled && Bounds.Contains(inputState.MousePosition);
            var isClicked = isHovered && inputState.HasAction(ActionType.LeftWasClicked);
            _wasHovered = isHovered;

            Color = isDisabled ? _style.TextureDisableColor : isHovered ? _style.TextureHoverColor : _style.TextureIdleColor;
            if (_uiText is not null)
                _uiText.Color = isDisabled ? _style.TextDisableColor : isHovered ? _style.TextHoverColor : _style.TextIdleColor;

            if (isHovered && !_wasHovered)
                AudioService.SFX.PlaySound("hoverButton");

            if (!isClicked) return;
            AudioService.SFX.PlaySound("clickButton");
            OnClickAction?.Invoke();
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

    public class UiTextButton() : UiElement
    {
        private readonly ButtonStyle _style = new();
        private UiText _uiText;
        private bool _wasHovered;

        public UiText Text { set => _uiText = value; }
        public float Scale { set => _uiText.Scale = value; }
        public Action OnClickAction;
        public bool Disable;

        protected override void Updater(InputState inputState)
        {
            _uiText.Update(inputState, Bounds, UiScale);

            var isDisabled = OnClickAction is null || Disable;
            var isHovered = !isDisabled && Bounds.Contains(inputState.MousePosition);
            var isClicked = isHovered && inputState.HasAction(ActionType.LeftWasClicked);
            _wasHovered = isHovered;

            _uiText.Color = isDisabled ? _style.TextDisableColor : isHovered ? _style.TextHoverColor : _style.TextIdleColor;

            if (isHovered && !_wasHovered)
                AudioService.SFX.PlaySound("hoverButton");

            if (!isClicked) return;
            AudioService.SFX.PlaySound("clickButton");
            OnClickAction?.Invoke();
        }

        public void OveideColor(Color color)
        {
            _uiText.Color = color;
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
