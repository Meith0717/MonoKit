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
    public struct ButtonStyle
    {
        public Color TextIdleColor;
        public Color TextHoverColor;
        public Color TextDisableColor;
        public Color TextureIdleColor;
        public Color TextureHoverColor;
        public Color TextureDisableColor;
    }

    public class UiButton : UiFrame
    {
        private readonly static Color _textIdleColor = Color.White;
        private readonly static Color _textHoverColor = Color.Gray;
        private readonly static Color _textDisableColor = Color.DarkGray;
        private readonly static Color _textureIdleColor = Color.White;
        private readonly static Color _textureHoverColor = Color.MonoGameOrange;
        private readonly static Color _textureDisableColor = Color.DarkGray;

        private readonly UiText _uiText;
        public Action OnClickAction { get; set; }

        public Anchor TextAlign { set { _uiText.Anchor = value; } }
        public string Text { set { _uiText.Text = value; } }
        public float TextScale { set { _uiText.Scale = value; } }

        public bool Disable { get; set; } = false;

        public UiButton(string spriteFont, string text, string texture)
        {
            if (spriteFont != null)
                Add(_uiText = new(spriteFont) { Text = text, HSpace = 25, Anchor = Anchor.Center });
            Texture = texture;
        }

        private bool _wasHovered;
        protected override void Updater(InputState inputState)
        {
            base.Updater(inputState);

            var isDisabled = OnClickAction is null || Disable;
            var isHovered = !isDisabled && Bounds.Contains(inputState.MousePosition);
            var isClicked = isHovered && inputState.HasAction(ActionType.LeftWasClicked);

            Color = isDisabled ? _textureDisableColor : isHovered ? _textureHoverColor : _textureIdleColor;
            if (_uiText is not null)
                _uiText.Color = isDisabled ? _textDisableColor : isHovered ? _textHoverColor : _textIdleColor;

            if (isHovered && !_wasHovered)
                AudioService.SFX.PlaySound("hoverButton");

            if (isClicked)
            {
                AudioService.SFX.PlaySound("clickButton");
                OnClickAction?.Invoke();
            }

            _wasHovered = isHovered;
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            base.Drawer(spriteBatch);
        }
    }
}
