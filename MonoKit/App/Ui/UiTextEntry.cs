// AppHost - UiTextEntry.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

/// <summary>
///     Single-line text entry: click to focus, drag-free cursor navigation, password masking.
///     Characters arrive from the platform text-input event (see <see cref="InputHandler.EnableTextInput" />),
///     so keyboard layout, modifiers and key repeat are handled by the OS.
/// </summary>
public sealed class UiTextEntry(SpriteFont spriteFont) : UiElement
{
    private readonly SpriteFont _font = spriteFont;
    private int _cursorPosition;
    private bool _isFocused;
    private string _placeholder = string.Empty;
    private KeyboardState _previousKeyboardState;
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            _cursorPosition = MathHelper.Clamp(_cursorPosition, 0, _text.Length);
        }
    }

    public string Placeholder
    {
        get => _placeholder;
        set => _placeholder = value ?? string.Empty;
    }

    public float Scale { get; set; } = 1f;
    public Color Color { get; set; } = Color.White;
    public Color PlaceholderColor { get; set; } = Color.DimGray;
    public Color BgColor { get; set; } = new(40, 40, 40, 200);
    public Color FocusedBgColor { get; set; } = new(60, 60, 60, 220);
    public Color CursorColor { get; set; } = Color.White;
    public int Padding { get; set; } = 5;

    public bool IsPassword { get; set; }
    public char PasswordChar { get; set; } = '*';

    /// <summary>Sizes the element to fit text or placeholder plus padding.</summary>
    protected override void MeasureSize()
    {
        var displayText = PasswordAware(_text.Length > 0 ? _text : _placeholder);
        var textDimension = _font.MeasureString(displayText);
        IntrinsicWidth = (int)(textDimension.X * Scale) + Padding * 2;
        IntrinsicHeight = (int)(MathHelper.Max(textDimension.Y, _font.LineSpacing) * Scale) + Padding * 2;
    }

    /// <summary>Handles focus on click and edits while focused.</summary>
    protected override void Updater(InputHandler inputHandler)
    {
        var mouseState = Mouse.GetState();
        var keyboardState = Keyboard.GetState();

        // Click to focus/unfocus; clicking inside also moves the cursor to the click position.
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            _isFocused = Bounds.Contains(mouseState.Position);
            if (_isFocused)
                _cursorPosition = GetCursorPositionFromX(mouseState.X - Bounds.X - Padding);
        }

        if (_isFocused)
            HandleEditingKeys(keyboardState, inputHandler.TypedCharacters);

        _previousKeyboardState = keyboardState;
    }

    /// <summary>Applies navigation, editing keys, and typed characters.</summary>
    private void HandleEditingKeys(KeyboardState keyboardState, IReadOnlyList<char> typedCharacters)
    {
        if (IsKeyDownEdge(Keys.Enter) || IsKeyDownEdge(Keys.Escape))
        {
            _isFocused = false;
            return;
        }

        if (IsKeyDownEdge(Keys.Back) && _cursorPosition > 0)
        {
            _text = _text.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
        }

        if (IsKeyDownEdge(Keys.Delete) && _cursorPosition < _text.Length)
            _text = _text.Remove(_cursorPosition, 1);

        if (IsKeyDownEdge(Keys.Left))
            _cursorPosition = Math.Max(0, _cursorPosition - 1);
        if (IsKeyDownEdge(Keys.Right))
            _cursorPosition = Math.Min(_text.Length, _cursorPosition + 1);
        if (IsKeyDownEdge(Keys.Home))
            _cursorPosition = 0;
        if (IsKeyDownEdge(Keys.End))
            _cursorPosition = _text.Length;

        foreach (var character in typedCharacters)
        {
            _text = _text.Insert(_cursorPosition, character.ToString());
            _cursorPosition++;
        }

        return;

        bool IsKeyDownEdge(Keys key) =>
            keyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }

    /// <summary>Draws background, text, and the cursor when focused.</summary>
    protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        spriteBatch.FillRectangle(Bounds, _isFocused ? FocusedBgColor : BgColor);

        var hasText = _text.Length > 0;
        var displayText = hasText ? PasswordAware(_text) : _placeholder;
        var position = Bounds.Location.ToVector2();
        position.X += Padding;
        position.Y += Padding;

        if (!string.IsNullOrEmpty(displayText))
            spriteBatch.DrawString(
                _font,
                displayText,
                position,
                hasText ? Color : PlaceholderColor,
                0,
                Vector2.Zero,
                UiScale * Scale,
                SpriteEffects.None,
                1
            );

        if (!_isFocused)
            return;

        var cursorX =
            position.X
            + _font.MeasureString(displayText.Substring(0, _cursorPosition)).X * UiScale * Scale;
        var cursorHeight = _font.MeasureString("A").Y * UiScale * Scale;

        spriteBatch.DrawLine(
            new Vector2(cursorX, position.Y),
            new Vector2(cursorX, position.Y + cursorHeight),
            CursorColor,
            2f
        );
    }

    /// <summary>Masks the text with PasswordChar when IsPassword is set.</summary>
    private string PasswordAware(string text) =>
        IsPassword ? new string(PasswordChar, text.Length) : text;

    /// <summary>Maps a pixel x-offset to the nearest cursor index.</summary>
    private int GetCursorPositionFromX(float x)
    {
        if (_text.Length == 0)
            return 0;

        var displayText = PasswordAware(_text);
        for (var i = 0; i <= displayText.Length; i++)
        {
            var width = _font.MeasureString(displayText.Substring(0, i)).X * UiScale * Scale;
            if (width > x)
                return MathHelper.Clamp(i - 1, 0, _text.Length);
        }

        return _text.Length;
    }
}
