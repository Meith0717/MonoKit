// UiVariableSelector.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameEngine.Ui
{
    public class UiVariableSelector<T> : UiElement
    {
        private readonly UiButton _arrowL;
        private readonly UiButton _arrowR;
        private readonly UiText _text;
        private readonly List<T> _items;
        private int _selectedIndex;

        public float TextScale { set { _text.Scale = value; } }
        public float ButtonScale { set { _text.Scale = value; } }
        public Color TextColor { set { _text.Color = value; } }
        public bool Disable { set { _arrowL.Disable = _arrowR.Disable = value; } }
        public T Value
        {
            set => _selectedIndex = GetIndex(value);
            get => _items[_selectedIndex];
        }

        public UiVariableSelector(string spriteFont, T[] items)
        {
            _items = [.. items];

            _arrowL = new UiButton("ui_arrow_l")
            {
                OnClickAction = DecreaseIndex,
                Anchor = Anchor.W
            };

            _arrowR = new UiButton("ui_arrow_r")
            {
                OnClickAction = IncreaseIndex,
                Anchor = Anchor.E
            };

            _text = new(spriteFont)
            {
                Anchor = Anchor.Center,
                Text = _items[_selectedIndex].ToString()
            };
        }

        protected override void Updater(InputState inputState)
        {
            _arrowL.Update(inputState, Bounds, UiScale);
            _arrowR.Update(inputState, Bounds, UiScale);
            _text.Update(inputState, Bounds, UiScale);
            _text.Text = _items[_selectedIndex].ToString();
            Height = int.Max(_text.Bounds.Height, int.Max(_arrowL.Bounds.Height, _arrowR.Bounds.Height));
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            _arrowL.Draw(spriteBatch);
            _arrowR.Draw(spriteBatch);
            _text.Draw(spriteBatch);
        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);
            _arrowL.ApplyScale(Bounds, UiScale);
            _arrowR.ApplyScale(Bounds, UiScale);
            _text.ApplyScale(Bounds, UiScale);
        }

        public override void Dispose()
        {
            base.Dispose();
            _arrowL.Dispose();
            _arrowR.Dispose();
            _text.Dispose();
        }

        private void IncreaseIndex()
        {
            _selectedIndex++;
            if (_selectedIndex >= _items.Count)
                _selectedIndex = 0;
            _text.Text = _items[_selectedIndex].ToString();
        }

        private void DecreaseIndex()
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _items.Count - 1;
            _text.Text = _items[_selectedIndex].ToString();
        }

        private int GetIndex(T value)
        {
            var index = _items.IndexOf(value);
            if (index > -1)
                return index;
            throw new KeyNotFoundException($"{value} was not found.");
        }
    }
}

