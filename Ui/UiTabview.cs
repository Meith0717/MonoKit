// UiTabView.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameEngine.Ui
{
    public class UiTabView : UiElement
    {
        private readonly Dictionary<UiButton, UiFrame> _tabs = new();
        private UiFrame _activeFrame;

        public void Add(string tabDescription, UiFrame tabFrame)
            => _tabs.Add(new UiButton("default_font", tabDescription), tabFrame);

        public void Initialize()
        {
            var tabCount = _tabs.Count;
            var i = 0;

            foreach (var tab in _tabs)
            {
                var tabFrame = tab.Value;
                tabFrame.RelWidth = 1;
                tabFrame.RelHeight = .95f;

                var tabButton = tab.Key;
                tabButton.OnClickAction = () => _activeFrame = tabFrame;
                tabButton.RelY = 0;
                tabButton.RelWidth = 1f / tabCount;
                tabButton.RelHeight = .05f;
                tabButton.RelX = i * (1f / tabCount);
                tabButton.Color = Color.Transparent;
                tabButton.TextScale = .2f;
                tabButton.Alpha = 0;
                i++;

                _activeFrame ??= tabFrame;
            }
        }

        protected override void Updater(InputState inputState)
        {
            foreach (var tab in _tabs)
            {
                var button = tab.Key;
                var frame = tab.Value;
                button.Update(inputState, Bounds, UiScale);

                button.TextIdleColor = Color.White;
                if (ReferenceEquals(_activeFrame, frame))
                    button.TextIdleColor = Color.Blue;
            }

            if (_activeFrame is null) return;
            _activeFrame.Anchor = Anchor.S;
            _activeFrame.Update(inputState, Bounds, UiScale);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            _activeFrame?.Draw(spriteBatch);

            foreach (var button in _tabs.Keys)
                button.Draw(spriteBatch);
        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);

            foreach (var button in _tabs.Keys)
                button.ApplyScale(Bounds, uiScale);

            foreach (var tab in _tabs.Values)
                tab.ApplyScale(Bounds, uiScale);
        }
    }
}
