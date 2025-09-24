// UiTabview.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace GameEngine.Ui
{
    public sealed class UiTabView(Color color) : UiElement
    {
        private readonly List<(UiTextButton, UiFrame)> _tabs = new();
        private readonly Color _color = color;
        private UiFrame _activeFrame;

        public void Add(string tabDescription, UiFrame tabFrame)
            => _tabs.Add((new UiTextButton() { Text = new("default_font", tabDescription) { Scale = .25f, Allign = Allign.Center, HSpace = 10 } }, tabFrame));

        public void Initialize()
        {
            var tabCount = _tabs.Count;

            for (var i = 0; i < _tabs.Count; i++)
            {
                var (button, frame) = _tabs[i];
                _activeFrame ??= frame;

                frame.RelWidth = 1f;
                frame.RelHeight = .9f;
                frame.Allign = Allign.S;

                button.OnClickAction = () => _activeFrame = frame;
                button.RelX = i * (1f / tabCount);
                button.RelY = 0;
                button.RelWidth = 1f / tabCount;
                button.RelHeight = .1f;
            }
        }

        protected override void Updater(InputState inputState)
        {
            foreach (var (button, frame) in _tabs)
            {
                button.Update(inputState, Bounds, UiScale);
                if (_activeFrame == frame)
                    button.OveideColor(Color.MonoGameOrange);
            }

            if (_activeFrame is null) return;
            _activeFrame.Update(inputState, Bounds, UiScale);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(Bounds, _color);
            _activeFrame?.Draw(spriteBatch);

            foreach (var (button, _) in _tabs)
                button.Draw(spriteBatch);

        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);

            foreach (var (button, frame) in _tabs)
            {
                button.ApplyScale(Bounds, uiScale);
                frame.ApplyScale(Bounds, uiScale);
            }
        }
    }
}
