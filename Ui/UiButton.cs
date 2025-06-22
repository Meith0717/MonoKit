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
    public enum TextAlign { W, E, Center }

    public class UiButton : UiFrame
    {
        private readonly UiText _uiText;
        public Action OnClickAction { get; set; }

        public Anchor TextAlign { set { _uiText.Anchor = value; } }
        public string Text { set { _uiText.Text = value; } }
        public float TextScale { set { _uiText.Scale = value; } }
        public float TextAlpha { set { _uiText.Alpha = value; } }

        public Color TextIdleColor { private get; set; } = Color.White;
        public Color TextHoverColor { private get; set; } = Color.Cyan;
        public Color TextDisableColor { private get; set; } = Color.DarkRed;
        public Color TextureIdleColor { private get; set; } = Color.White;
        public Color TextureHoverColor { private get; set; } = Color.Cyan;
        public Color TextureDisableColor { private get; set; } = Color.DarkGray;
        public bool Disable { private get; set; } = false;
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

        private bool _isHovered;
        private bool _isClicked;
        private bool _IsDisabled;

        protected override void Updater(InputState inputState)
        {
            base.Updater(inputState);

            if (_uiText is not null)
                _uiText.Color = _IsDisabled ? TextDisableColor : _isHovered ? TextHoverColor : TextIdleColor;
            Color = _IsDisabled ? TextureDisableColor : _isHovered ? TextureHoverColor : TextureIdleColor;

            if (_IsDisabled || Disable) return;

            if (Bounds.Contains(inputState.MousePosition) && !_isHovered)
                AudioService.SFX.PlaySound("hoverButton");

            _isHovered = Bounds.Contains(inputState.MousePosition);
            _isClicked = _isHovered && inputState.HasAction(ActionType.LeftWasClicked);
            _IsDisabled = OnClickAction is null;

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
            var font = ContentProvider.Fonts.Get("defaultFont");
            var pos = new Vector2(Bounds.Left, Bounds.Bottom);
            spriteBatch.DrawString(font, Tooltip, pos, Color.White, 0, Vector2.Zero, .1f, SpriteEffects.None, 1);
        }
    }
}
