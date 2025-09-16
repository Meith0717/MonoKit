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
        public string Tooltip { private get; set; }

        public UiButton(string spriteFont, string text, string texture)
        {
            Add(_uiText = new(spriteFont) { Text = text, HSpace = 25 });
            TextAlign = Anchor.Center;
            Texture = texture;
        }

        public UiButton(string spriteFont, string text)
        {
            Add(_uiText = new(spriteFont) { Text = text, HSpace = 10 });
            TextAlign = Anchor.Center;
        }

        public UiButton(string texture) => Texture = texture;

        public UiButton() { }

        private bool _isHovered;
        private bool _isClicked;
        private bool _IsDisabled;

        protected override void Updater(InputState inputState)
        {
            base.Updater(inputState);
            _isHovered = Bounds.Contains(inputState.MousePosition);
            _isClicked = _isHovered && inputState.HasAction(ActionType.LeftWasClicked);
            _IsDisabled = OnClickAction is null || Disable;
            if (_uiText is not null)
                _uiText.Color = _IsDisabled ? _textDisableColor : _isHovered ? _textHoverColor : _textIdleColor;
            Color = _IsDisabled ? _textDisableColor : _isHovered ? _textHoverColor : _textureIdleColor;
            if (_IsDisabled)
                return;

            if (Bounds.Contains(inputState.MousePosition) && !_isHovered)
                AudioService.SFX.PlaySound("hoverButton");

            if (!_isClicked) return;
            AudioService.SFX.PlaySound("clickButton");
            OnClickAction?.Invoke();
            _isHovered = false;
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            base.Drawer(spriteBatch);
            if (Tooltip is null) return;
            if (!_isHovered) return;
            var font = ContentProvider.Fonts.Get("default_font");
            var pos = new Vector2(Bounds.Left, Bounds.Bottom);
            spriteBatch.DrawString(font, Tooltip, pos, Color.White, 0, Vector2.Zero, .1f, SpriteEffects.None, 1);
        }
    }
}
