// MonoKit - UiSlider.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

/// <summary>Slider element filled proportionally to its 0-to-1 value.</summary>
public sealed class UiSlider(bool interactive) : UiElement
{
    private bool _hovered;
    private bool _pressed;
    private float _sliderValue;

    public float Value
    {
        get => _sliderValue;
        set => _sliderValue = float.Round(float.Clamp(value, 0, 1), 2);
    }

    public Color BgColor { private get; set; } = new(20, 20, 20);
    public Color Color { private get; set; } = Color.MonoGameOrange;
    public Color HoverColor { private get; set; } = Color.Orange;

    /// <summary>Handles hover, drag, and value tracking from the mouse.</summary>
    protected override void Updater(InputHandler inputHandler)
    {
        if (!interactive)
            return;
        var mousePos = Mouse.GetState().Position;
        _hovered = Bounds.Contains(mousePos);
        // Drag only a press that started on this slider (capture, not hover).
        _pressed = IsMouseHold;
        if (_pressed)
        {
            var rectangle = Bounds;
            var ratio = rectangle.Width / rectangle.Height;
            if (ratio < 1) // Vertical
            {
                float relativeMousePosition = Bounds.Bottom - mousePos.Y;
                _sliderValue = relativeMousePosition / Bounds.Height;
            }
            else // Horizontal
            {
                float relativeMousePosition = mousePos.X - Bounds.X;
                _sliderValue = relativeMousePosition / Bounds.Width;
            }

            _sliderValue = float.Round(float.Clamp(_sliderValue, 0, 1), 2);
        }
    }

    /// <summary>Draws the background and the filled value portion.</summary>
    protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        var rectangle = Bounds;
        spriteBatch.FillRectangle(rectangle, BgColor);

        var ratio = rectangle.Height == 0 ? 0 : rectangle.Width / rectangle.Height;
        if (ratio < 1)
        {
            var bottom = rectangle.Bottom;
            rectangle.Height = (int)(rectangle.Height * _sliderValue);
            rectangle.Y = bottom - rectangle.Height;
        }
        else
        {
            rectangle.Width = (int)(rectangle.Width * _sliderValue);
        }

        spriteBatch.FillRectangle(rectangle, _hovered || _pressed ? HoverColor : Color);
    }
}
