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
    public class UiButton(string texture) : UiSprite(texture)
    {
        private readonly static Color _textIdleColor = Color.White;
        private readonly static Color _textHoverColor = Color.Gray;
        private readonly static Color _textDisableColor = Color.DarkGray;
        private readonly static Color _textureIdleColor = Color.White;
        private readonly static Color _textureHoverColor = Color.MonoGameOrange;
        private readonly static Color _textureDisableColor = Color.DarkGray;

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

            Color = isDisabled ? _textureDisableColor : isHovered ? _textureHoverColor : _textureIdleColor;
            if (_uiText is not null)
                _uiText.Color = isDisabled ? _textDisableColor : isHovered ? _textHoverColor : _textIdleColor;

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
}
