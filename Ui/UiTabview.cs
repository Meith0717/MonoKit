// UiTabview.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Input;
using System.Collections.Generic;

namespace MonoKit.Ui
{
    public enum TabAxix : byte { X, Y }

    public sealed class UiTabView<ButtonType>(Color color) : UiElement where ButtonType : UiElement, IButton
    {
        private readonly List<(UiFrame, ButtonType, UiFrame)> _tabs = new();
        private readonly Color _color = color;
        private UiFrame _activeFrame;

        public void Add(ButtonType button, UiFrame tabFrame)
        {
            var buttonFrame = new UiFrame() { Color = Color.Transparent };
            buttonFrame.Add(button);
            button.OnClickAction += () => _activeFrame = tabFrame;

            _tabs.Add((buttonFrame, button, tabFrame));
        }

        public void Initialize(TabAxix axix, float tabButtonRatio)
        {
            var tabCount = _tabs.Count;

            if (axix == TabAxix.Y)
            {
                for (var i = 0; i < _tabs.Count; i++)
                {
                    var (buttonFrame, _, frame) = _tabs[i];
                    _activeFrame ??= frame;

                    frame.RelWidth = 1f - tabButtonRatio;
                    frame.RelHeight = 1f;
                    frame.Allign = Allign.E;

                    buttonFrame.RelX = 0;
                    buttonFrame.RelY = i * (1f / 10);
                    buttonFrame.RelWidth = tabButtonRatio;
                    buttonFrame.RelHeight = 1f / 10;
                }
            }
            else if (axix == TabAxix.X)
            {
                for (var i = 0; i < _tabs.Count; i++)
                {
                    var (buttonFrame, _, frame) = _tabs[i];
                    _activeFrame ??= frame;

                    frame.RelWidth = 1f;
                    frame.RelHeight = 1f - tabButtonRatio;
                    frame.Allign = Allign.S;

                    buttonFrame.RelX = i * (1f / tabCount);
                    buttonFrame.RelY = 0;
                    buttonFrame.RelWidth = 1f / tabCount;
                    buttonFrame.RelHeight = tabButtonRatio;
                }
            }
        }

        protected override void Updater(InputHandler inputHandler)
        {
            foreach (var (buttonFrame, button, frame) in _tabs)
            {
                buttonFrame.Update(inputHandler, Bounds, UiScale);
                if (_activeFrame == frame)
                    button.OveideColor(Color.Red, Color.Red);
            }

            if (_activeFrame is null) return;
            _activeFrame.Update(inputHandler, Bounds, UiScale);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(Bounds, _color);
            _activeFrame?.Draw(spriteBatch);

            foreach (var (buttonFrame, _, _) in _tabs)
                buttonFrame.Draw(spriteBatch);

        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);

            foreach (var (buttonFrame, _, frame) in _tabs)
            {
                buttonFrame.ApplyScale(Bounds, uiScale);
                frame.ApplyScale(Bounds, uiScale);
            }
        }
    }
}
