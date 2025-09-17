// UiTabview.cs 
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
            => _tabs.Add(new UiButton("button")
            {
                Text = new("default_font", tabDescription) { Scale = .2f }
            }, tabFrame);

        public void Initialize()
        {
            var tabCount = _tabs.Count;
            var i = 0;

            foreach (var tab in _tabs)
            {
                var tabFrame = tab.Value;
                tabFrame.RelWidth = .85f;
                tabFrame.RelHeight = 1f;

                var tabButton = tab.Key;
                tabButton.OnClickAction = () => _activeFrame = tabFrame;
                tabButton.RelY = i * (1f / tabCount);
                tabButton.RelWidth = .15f;
                tabButton.RelHeight = 1f / tabCount;
                tabButton.RelX = 0;
                i++;

                _activeFrame ??= tabFrame;
            }
        }

        protected override void Updater(InputState inputState)
        {
            foreach (var tab in _tabs)
            {
                var button = tab.Key;
                button.Update(inputState, Bounds, UiScale);
            }

            if (_activeFrame is null) return;
            _activeFrame.Anchor = Anchor.E;
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
