// UiTextEntry.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoKit.Content;
using MonoKit.Input;

namespace MonoKit.Ui;

/// <summary>
/// A text entry field for the UI system.
/// Supports click-to-focus, cursor navigation, selection, backspace, delete.
/// </summary>
public sealed class UiTextEntry : UiElement
{
    private readonly SpriteFont _font;
    private string _text = string.Empty;
    private string _placeholder = string.Empty;
    private bool _isFocused;
    private bool _showCursor;
    private int _cursorPosition;
    private int _selectionStart = -1;
    private int _selectionLength;

    // Track previous keyboard state for edge detection
    private KeyboardState _previousKeyboardState;

    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            _cursorPosition = MathHelper.Clamp(_cursorPosition, 0, _text.Length);
            UpdateSize();
        }
    }

    public string Placeholder
    {
        get => _placeholder;
        set
        {
            _placeholder = value ?? string.Empty;
            UpdateSize();
        }
    }

    public float Scale { get; set; } = 1f;
    public Color Color { get; set; } = Color.White;
    public Color PlaceholderColor { get; set; } = Color.DimGray;
    public Color BgColor { get; set; } = new Color(40, 40, 40, 200);
    public Color FocusedBgColor { get; set; } = new Color(60, 60, 60, 220);
    public Color CursorColor { get; set; } = Color.White;
    public int Padding { get; set; } = 5;

    public bool IsPassword { get; set; }
    public char PasswordChar { get; set; } = '*';

    public UiTextEntry(string spriteFont)
    {
        _font = ContentProvider.Container<SpriteFont>().Get(spriteFont);
        _previousKeyboardState = Keyboard.GetState();
        UpdateSize();
    }

    public override void ApplyScale(Rectangle root, float uiScale)
    {
        base.ApplyScale(root, uiScale);
        UpdateSize();
    }

    protected override void Updater(InputHandler inputHandler)
    {
        var mouseState = Mouse.GetState();
        var mousePos = mouseState.Position;
        var keyboardState = Keyboard.GetState();

        // Handle focus on click
        var wasMousePressed = mouseState.LeftButton == ButtonState.Pressed;
        
        if (Bounds.Contains(mousePos) && wasMousePressed)
        {
            _isFocused = true;
            // Move cursor to clicked position
            var relativeX = mousePos.X - Bounds.X - Padding;
            _cursorPosition = GetCursorPositionFromX(relativeX);
            _selectionStart = -1;
            _selectionLength = 0;
        }
        else if (wasMousePressed)
        {
            _isFocused = false;
        }

        if (_isFocused)
        {
            _showCursor = true; // Always show cursor when focused

            // Handle keyboard input using edge detection
            var currentKeys = keyboardState.GetPressedKeys();
            var previousKeys = _previousKeyboardState.GetPressedKeys();

            // Check for Enter (unfocus)
            if (IsKeyPressed(Keys.Enter, keyboardState, _previousKeyboardState))
            {
                _isFocused = false;
            }

            // Check for Backspace
            if (IsKeyPressed(Keys.Back, keyboardState, _previousKeyboardState))
            {
                HandleBackspace();
            }

            // Check for Delete
            if (IsKeyPressed(Keys.Delete, keyboardState, _previousKeyboardState))
            {
                HandleDelete();
            }

            // Check for Left arrow
            if (IsKeyPressed(Keys.Left, keyboardState, _previousKeyboardState))
            {
                bool shiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
                if (shiftPressed)
                {
                    if (_selectionStart == -1)
                        _selectionStart = _cursorPosition;
                    _cursorPosition = Math.Max(0, _cursorPosition - 1);
                    _selectionLength = Math.Abs(_selectionStart - _cursorPosition);
                }
                else
                {
                    _cursorPosition = Math.Max(0, _cursorPosition - 1);
                    _selectionStart = -1;
                    _selectionLength = 0;
                }
            }

            // Check for Right arrow
            if (IsKeyPressed(Keys.Right, keyboardState, _previousKeyboardState))
            {
                bool shiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
                if (shiftPressed)
                {
                    if (_selectionStart == -1)
                        _selectionStart = _cursorPosition;
                    _cursorPosition = Math.Min(_text.Length, _cursorPosition + 1);
                    _selectionLength = Math.Abs(_selectionStart - _cursorPosition);
                }
                else
                {
                    _cursorPosition = Math.Min(_text.Length, _cursorPosition + 1);
                    _selectionStart = -1;
                    _selectionLength = 0;
                }
            }

            // Check for Home
            if (IsKeyPressed(Keys.Home, keyboardState, _previousKeyboardState))
            {
                bool shiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
                if (shiftPressed)
                {
                    if (_selectionStart == -1)
                        _selectionStart = _cursorPosition;
                    _cursorPosition = 0;
                    _selectionLength = Math.Abs(_selectionStart - _cursorPosition);
                }
                else
                {
                    _cursorPosition = 0;
                    _selectionStart = -1;
                    _selectionLength = 0;
                }
            }

            // Check for End
            if (IsKeyPressed(Keys.End, keyboardState, _previousKeyboardState))
            {
                bool shiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
                if (shiftPressed)
                {
                    if (_selectionStart == -1)
                        _selectionStart = _cursorPosition;
                    _cursorPosition = _text.Length;
                    _selectionLength = Math.Abs(_selectionStart - _cursorPosition);
                }
                else
                {
                    _cursorPosition = _text.Length;
                    _selectionStart = -1;
                    _selectionLength = 0;
                }
            }

            // Check for Escape (unfocus without changing text)
            if (IsKeyPressed(Keys.Escape, keyboardState, _previousKeyboardState))
            {
                _isFocused = false;
            }

            // Handle character input
            HandleCharacterInput(keyboardState, _previousKeyboardState);
        }
        else
        {
            _showCursor = false;
        }

        _previousKeyboardState = keyboardState;
    }

    private bool IsKeyPressed(Keys key, KeyboardState current, KeyboardState previous)
    {
        return current.IsKeyDown(key) && !previous.IsKeyDown(key);
    }

    private void HandleBackspace()
    {
        if (_selectionLength > 0)
        {
            _text = _text.Remove(_selectionStart, _selectionLength);
            _cursorPosition = _selectionStart;
            _selectionLength = 0;
            _selectionStart = -1;
        }
        else if (_cursorPosition > 0)
        {
            _text = _text.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
        }
        UpdateSize();
    }

    private void HandleDelete()
    {
        if (_selectionLength > 0)
        {
            _text = _text.Remove(_selectionStart, _selectionLength);
            _cursorPosition = _selectionStart;
            _selectionLength = 0;
            _selectionStart = -1;
        }
        else if (_cursorPosition < _text.Length)
        {
            _text = _text.Remove(_cursorPosition, 1);
        }
        UpdateSize();
    }

    private void HandleCharacterInput(KeyboardState current, KeyboardState previous)
    {
        // Check all keys that were just pressed
        var currentPressed = current.GetPressedKeys();
        var previousPressed = previous.GetPressedKeys();

        foreach (var key in currentPressed)
        {
            // Skip if this key was also pressed before
            if (Array.IndexOf(previousPressed, key) >= 0)
                continue;

            // Skip modifier and special keys
            if (IsModifierOrSpecialKey(key))
                continue;

            // Get character from key
            var character = GetCharFromKey(key, current);
            if (character != '\0')
            {
                // Insert character at cursor position
                if (_selectionLength > 0)
                {
                    _text = _text.Remove(_selectionStart, _selectionLength);
                    _cursorPosition = _selectionStart;
                    _selectionLength = 0;
                    _selectionStart = -1;
                }
                _text = _text.Insert(_cursorPosition, character.ToString());
                _cursorPosition++;
                UpdateSize();
                break; // Only process one key per frame
            }
        }
    }

    private bool IsModifierOrSpecialKey(Keys key)
    {
        return key switch
        {
            Keys.LeftShift or Keys.RightShift or
            Keys.LeftControl or Keys.RightControl or
            Keys.LeftAlt or Keys.RightAlt or
            Keys.LeftWindows or Keys.RightWindows or
            Keys.CapsLock or Keys.NumLock or
            Keys.PrintScreen or
            Keys.Left or Keys.Right or Keys.Up or Keys.Down or
            Keys.Home or Keys.End or
            Keys.PageUp or Keys.PageDown or
            Keys.Back or Keys.Delete or
            Keys.Tab or Keys.Enter or
            Keys.Escape => true,
            _ => false,
        };
    }

    private char GetCharFromKey(Keys key, KeyboardState keyboardState)
    {
        bool shiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
        bool capsLock = keyboardState.CapsLock;
        bool effectiveShift = shiftPressed ^ capsLock;

        return key switch
        {
            Keys.A => effectiveShift ? 'A' : 'a',
            Keys.B => effectiveShift ? 'B' : 'b',
            Keys.C => effectiveShift ? 'C' : 'c',
            Keys.D => effectiveShift ? 'D' : 'd',
            Keys.E => effectiveShift ? 'E' : 'e',
            Keys.F => effectiveShift ? 'F' : 'f',
            Keys.G => effectiveShift ? 'G' : 'g',
            Keys.H => effectiveShift ? 'H' : 'h',
            Keys.I => effectiveShift ? 'I' : 'i',
            Keys.J => effectiveShift ? 'J' : 'j',
            Keys.K => effectiveShift ? 'K' : 'k',
            Keys.L => effectiveShift ? 'L' : 'l',
            Keys.M => effectiveShift ? 'M' : 'm',
            Keys.N => effectiveShift ? 'N' : 'n',
            Keys.O => effectiveShift ? 'O' : 'o',
            Keys.P => effectiveShift ? 'P' : 'p',
            Keys.Q => effectiveShift ? 'Q' : 'q',
            Keys.R => effectiveShift ? 'R' : 'r',
            Keys.S => effectiveShift ? 'S' : 's',
            Keys.T => effectiveShift ? 'T' : 't',
            Keys.U => effectiveShift ? 'U' : 'u',
            Keys.V => effectiveShift ? 'V' : 'v',
            Keys.W => effectiveShift ? 'W' : 'w',
            Keys.X => effectiveShift ? 'X' : 'x',
            Keys.Y => effectiveShift ? 'Y' : 'y',
            Keys.Z => effectiveShift ? 'Z' : 'z',
            Keys.D0 => shiftPressed ? ')' : '0',
            Keys.D1 => shiftPressed ? '!' : '1',
            Keys.D2 => shiftPressed ? '@' : '2',
            Keys.D3 => shiftPressed ? '#' : '3',
            Keys.D4 => shiftPressed ? '$' : '4',
            Keys.D5 => shiftPressed ? '%' : '5',
            Keys.D6 => shiftPressed ? '^' : '6',
            Keys.D7 => shiftPressed ? '&' : '7',
            Keys.D8 => shiftPressed ? '*' : '8',
            Keys.D9 => shiftPressed ? '(' : '9',
            Keys.NumPad0 => '0',
            Keys.NumPad1 => '1',
            Keys.NumPad2 => '2',
            Keys.NumPad3 => '3',
            Keys.NumPad4 => '4',
            Keys.NumPad5 => '5',
            Keys.NumPad6 => '6',
            Keys.NumPad7 => '7',
            Keys.NumPad8 => '8',
            Keys.NumPad9 => '9',
            Keys.OemPeriod => '.',
            Keys.OemComma => ',',
            Keys.OemQuestion => shiftPressed ? '?' : '/',
            Keys.OemSemicolon => shiftPressed ? ':' : ';',
            Keys.OemQuotes => shiftPressed ? '"' : '\'',
            Keys.OemOpenBrackets => shiftPressed ? '{' : '[',
            Keys.OemCloseBrackets => shiftPressed ? '}' : ']',
            Keys.OemBackslash => shiftPressed ? '|' : '\\',
            Keys.OemMinus => shiftPressed ? '_' : '-',
            Keys.OemPlus => shiftPressed ? '+' : '=',
            Keys.OemTilde => shiftPressed ? '~' : '`',
            Keys.Space => ' ',
            _ => '\0',
        };
    }

    protected override void Drawer(SpriteBatch spriteBatch)
    {
        // Draw background
        var bgColor = _isFocused ? FocusedBgColor : BgColor;
        spriteBatch.FillRectangle(Bounds, bgColor);

        // Draw text or placeholder
        var displayText = string.IsNullOrEmpty(_text) ? _placeholder : _text;
        var textColor = string.IsNullOrEmpty(_text) ? PlaceholderColor : Color;

        if (IsPassword && !string.IsNullOrEmpty(_text))
        {
            displayText = new string(PasswordChar, _text.Length);
        }

        var position = Bounds.Location.ToVector2();
        position.X += Padding;
        position.Y += Padding;

        spriteBatch.DrawString(
            _font,
            displayText,
            position,
            textColor,
            0,
            Vector2.Zero,
            UiScale * Scale,
            SpriteEffects.None,
            1
        );

        // Draw selection if any
        if (_selectionLength > 0 && _selectionStart != -1)
        {
            var start = Math.Min(_selectionStart, _cursorPosition);
            var length = _selectionLength;
            var selectionText = displayText.Substring(start, length);
            var selectionWidth = _font.MeasureString(selectionText).X * UiScale * Scale;
            var selectionX = position.X + _font.MeasureString(displayText.Substring(0, start)).X * UiScale * Scale;
            var selectionY = position.Y;
            var selectionHeight = _font.MeasureString("A").Y * UiScale * Scale;

            spriteBatch.FillRectangle(
                new Rectangle((int)selectionX, (int)selectionY, (int)selectionWidth, (int)selectionHeight),
                new Color(100, 100, 200, 150)
            );
        }

        // Draw cursor if focused
        if (_isFocused && _showCursor)
        {
            var cursorX = position.X + _font.MeasureString(displayText.Substring(0, _cursorPosition)).X * UiScale * Scale;
            var cursorY = position.Y;
            var cursorHeight = _font.MeasureString("A").Y * UiScale * Scale;

            spriteBatch.DrawLine(
                new Vector2(cursorX, cursorY),
                new Vector2(cursorX, cursorY + cursorHeight),
                CursorColor,
                2f
            );
        }
    }

    private void UpdateSize()
    {
        var displayText = string.IsNullOrEmpty(_text) ? _placeholder : _text;
        if (IsPassword && !string.IsNullOrEmpty(_text))
        {
            displayText = new string(PasswordChar, _text.Length);
        }

        var textDimension = _font.MeasureString(displayText);
        Width = (int)(textDimension.X * Scale) + Padding * 2;
        Height = (int)(textDimension.Y * Scale) + Padding * 2;
    }

    private int GetCursorPositionFromX(float x)
    {
        if (string.IsNullOrEmpty(_text))
            return 0;

        var displayText = IsPassword ? new string(PasswordChar, _text.Length) : _text;
        for (int i = 0; i <= displayText.Length; i++)
        {
            var substring = displayText.Substring(0, i);
            var width = _font.MeasureString(substring).X * UiScale * Scale;
            if (width > x)
            {
                return MathHelper.Clamp(i - 1, 0, _text.Length);
            }
        }
        return _text.Length;
    }
}
