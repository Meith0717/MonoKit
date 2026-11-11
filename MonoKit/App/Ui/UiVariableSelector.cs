// MonoKit - UiVariableSelector.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Ui;

/// <summary>
///     Value picker: arrow buttons on the left/right edges and a centered label.
///     A plain <see cref="UiContainer" /> — arrows and label are regular children,
///     so update/draw/scale/clip cascade is handled by the standard machinery.
/// </summary>
public sealed class UiVariableSelector<T> : UiContainer
{
    private readonly UiButton.Sprite _arrowL;
    private readonly UiButton.Sprite _arrowR;
    private readonly SpriteFont _font;
    private readonly UiText _text;
    private List<T> _items;
    private int _selectedIndex;

    /// <summary>Builds the arrow buttons and the centered value label.</summary>
    public UiVariableSelector(
        Texture2D leftButton,
        Texture2D rightButton,
        SpriteFont spriteFont,
        T[] items
    )
    {
        _font = spriteFont;
        _items = [.. items];

        _arrowL = new UiButton.Sprite(leftButton) { Align = Align.W };
        _arrowL.OnClickAction += DecreaseIndex;

        _arrowR = new UiButton.Sprite(rightButton) { Align = Align.E };
        _arrowR.OnClickAction += IncreaseIndex;

        _text = new UiText(spriteFont) { Align = Align.Center };
        UpdateText();

        Add(_arrowL);
        Add(_text);
        Add(_arrowR);
    }

    public Action<T> OnClickAction { get; set; }

    public T[] Items
    {
        set
        {
            _items = [.. value];
            _selectedIndex =
                _items.Count == 0 ? 0 : Math.Clamp(_selectedIndex, 0, _items.Count - 1);
            UpdateText();
        }
    }

    public float TextScale
    {
        set => _text.Scale = value;
    }

    public float ButtonScale
    {
        set => _arrowL.Scale = _arrowR.Scale = value;
    }

    public Color TextColor
    {
        set => _text.Color = value;
    }

    public bool Disable
    {
        set => _arrowL.Disable = _arrowR.Disable = value;
    }

    public T Value
    {
        get
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("UiVariableSelector has no items.");
            return _items[_selectedIndex];
        }
        set => _selectedIndex = GetIndex(value);
    }

    // Height derives from the tallest part (label or arrows), so parent boxes get a fitting row height.
    protected override void MeasureSize()
    {
        var arrowHeightL = _arrowL.Texture2D?.Height ?? 0;
        var arrowHeightR = _arrowR.Texture2D?.Height ?? 0;
        IntrinsicHeight = (int)
            Math.Max(_font.LineSpacing * _text.Scale, Math.Max(arrowHeightL, arrowHeightR));
    }

    // Arrows pinned to the edges, label centered — for all window sizes.
    protected override void LayoutChildren()
    {
        _arrowL.RelX = 0;
        _arrowL.RelY = 0;
        _arrowL.Align = Align.W;
        _arrowR.RelX = 0;
        _arrowR.RelY = 0;
        _arrowR.Align = Align.E;
        _text.RelX = 0;
        _text.RelY = 0;
        _text.Align = Align.Center;
    }

    /// <summary>Advances the selection to the next item, wrapping around.</summary>
    private void IncreaseIndex()
    {
        if (_items.Count <= 0)
            return;
        _selectedIndex++;
        if (_selectedIndex >= _items.Count)
            _selectedIndex = 0;
        UpdateText();
        OnClickAction?.Invoke(_items[_selectedIndex]);
    }

    /// <summary>Moves the selection to the previous item, wrapping around.</summary>
    private void DecreaseIndex()
    {
        if (_items.Count <= 0)
            return;
        _selectedIndex--;
        if (_selectedIndex < 0)
            _selectedIndex = _items.Count - 1;
        UpdateText();
        OnClickAction?.Invoke(_items[_selectedIndex]);
    }

    /// <summary>Syncs the label with the selected item.</summary>
    private void UpdateText()
    {
        _text.Text = _items.Count == 0 ? string.Empty : _items[_selectedIndex].ToString();
    }

    /// <summary>Returns the item's index; throws if the value is absent.</summary>
    private int GetIndex(T value)
    {
        var index = _items.IndexOf(value);
        if (index > -1)
            return index;
        throw new KeyNotFoundException($"{value} was not found.");
    }
}
