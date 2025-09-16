// UiVariableSelector.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GameEngine.Ui
{
    public class UiVariableSelector<T> : UiFrame
    {
        private readonly UiButton _arrowL;
        private readonly UiButton _arrowR;
        private readonly UiText _text;
        private List<T> _items = new();
        private int _selectedIndex;

        public UiVariableSelector(string spriteFont)
        {
            Add(_arrowL = new UiButton("ui_arrow_l")
            {
                OnClickAction = DecreaseIndex,
                Anchor = Anchor.W,
                TextureScale = .5f
            });

            Add(_arrowR = new UiButton("ui_arrow_r")
            {
                OnClickAction = IncreaseIndex,
                Anchor = Anchor.E,
                TextureScale = .5f
            });

            Add(_text = new(spriteFont)
            {
                Anchor = Anchor.Center,
                Text = "n.a."
            });

            Color = Color.Transparent;
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

        public float TextScale { set { _text.Scale = value; } }
        public float ButtonScale { set { _text.Scale = value; } }
        public Color TextColor { set { _text.Color = value; } }
        public float TextAlpha { set { _text.Alpha = value; } }
        public bool Disable { set  { _arrowL.Disable = _arrowR.Disable = value; } }
        public List<T> Values
        {
            set
            {
                if (value.Count == 0) return;
                if (value is null) return;
                _items = value;
                _selectedIndex = 0;
                _text.Text = _items[_selectedIndex].ToString();
            }
        }
        public T Value
        {
            get { return _items[_selectedIndex]; }
            set
            {
                if (value == null) return;
                if (_items.Contains(value))
                    _selectedIndex = _items.IndexOf(value);
                _text.Text = _items[_selectedIndex].ToString();
            }
        }

    }
}
