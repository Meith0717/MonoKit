// UiTabview.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.Ui
{
    public class UiTabview : UiElement
    {
        private readonly Dictionary<UiButton, UiFrame> _tabs = new();
        private readonly UiButton _leftButton;
        private readonly UiButton _rightButton;
        private UiFrame _activeFrame;

        public UiTabview()
        {
            _leftButton = new UiButton("arrowL")
            {
                Anchor = Anchor.NW,
                HSpace = 10,
                VSpace = 10

            };
            _rightButton = new UiButton("arrowR")
            {
                Anchor = Anchor.NE,
                HSpace = 10,
                VSpace = 10
            };
        }

        public void Add(string tabDescription, UiFrame tabFrame)
            => _tabs.Add(new UiButton("defaultFont", tabDescription), tabFrame);

        public void Initialize()
        {
            var tabCount = _tabs.Count;
            var i = 0;

            foreach (var tab in _tabs)
            {
                var tabButton = tab.Key;
                var tabFrame = tab.Value;
                tabFrame.RelWidth = 1;
                tabFrame.RelHeight = .95f;

                tabButton.OnClickAction = () => _activeFrame = tabFrame;
                tabButton.RelY = 0;
                tabButton.RelWidth = .8f / tabCount;
                tabButton.RelHeight = .05f;
                tabButton.RelX = i * (.8f / (tabCount)) + .1f;
                tabButton.Color = Color.Transparent;
                tabButton.TextScale = .2f;
                tabButton.Alpha = 0;
                i++;

                if (_activeFrame is null)
                    _activeFrame = tabFrame;
            }
        }

        protected override void Updater(InputState inputState)
        {
            foreach (var tab in _tabs)
            {
                var button = tab.Key;
                var frame = tab.Value;
                button.Update(inputState, Bounds);

                button.TextIdleColor = Color.White;
                if (ReferenceEquals(_activeFrame, frame))
                    button.TextIdleColor = Color.Blue;
            }

            if (_activeFrame is null) return;
            _activeFrame.Anchor = Anchor.S;
            _activeFrame.Update(inputState, Bounds);
            _leftButton.Update(inputState, Bounds);
            _rightButton.Update(inputState, Bounds);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            foreach (var button in _tabs.Keys)
                button.Draw(spriteBatch);
            _activeFrame?.Draw(spriteBatch);
            _leftButton.Draw(spriteBatch);
            _rightButton.Draw(spriteBatch);
        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);
            foreach (var button in _tabs.Keys)
                button.ApplyScale(Bounds, uiScale);
            foreach (var tab in _tabs.Values)
                tab.ApplyScale(Bounds, uiScale);
            _leftButton.ApplyScale(Bounds, uiScale);
            _rightButton.ApplyScale(Bounds, uiScale);
        }
    }
}
