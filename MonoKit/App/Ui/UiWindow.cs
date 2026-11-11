// MonoKit - UiWindow.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

/// <summary>
///     Draggable window with a title bar and a scrollable content layer below it.
///     Dragging uses the title bar (hold left mouse); scrolling and clipping of the content
///     are owned by the <see cref="UiScrollArea" /> content layer (wheel while hovered over it).
/// </summary>
public class UiWindow : UiContainer
{
    private readonly UiContainer _titleBar;
    private readonly UiText _titleText;

    public bool Draggable = true;

    /// <summary>Title bar height as a fraction of the window height.</summary>
    public float TitleBarHeight = 0.12f;

    private bool _dragging;
    private Point _lastMouse;

    /// <summary>Builds the title bar with its text and the scrollable content layer.</summary>
    public UiWindow(SpriteFont font, string title)
    {
        _titleText = new UiText(font, title) { Align = Align.W, Color = Color.White };
        _titleBar = new UiContainer { Color = new Color(40, 40, 40, 220) };
        _titleBar.Add(_titleText);

        Content = new UiScrollArea();

        Add(_titleBar);
        Add(Content);
    }

    public string Title
    {
        get => _titleText.Text;
        set => _titleText.Text = value;
    }

    /// <summary>Current scroll offset of the content layer in (unscaled) pixels.</summary>
    public float ScrollOffset => Content.ScrollOffset;

    /// <summary>Pixels scrolled per mouse-wheel unit (wheel delta is ±120 per notch).</summary>
    public float ScrollSpeed
    {
        get => Content.ScrollSpeed;
        set => Content.ScrollSpeed = value;
    }

    public Color TitleBarColor
    {
        get => _titleBar.Color;
        set => _titleBar.Color = value;
    }

    /// <summary>The scrollable content layer; children render clipped to its bounds.</summary>
    public UiScrollArea Content { get; }

    /// <summary>Adds a child to the scrollable content layer.</summary>
    public void AddContent(UiElement child)
    {
        Content.Add(child);
    }

    /// <summary>Positions the title bar and the content layer.</summary>
    protected override void LayoutChildren()
    {
        _titleBar.RelX = 0;
        _titleBar.RelY = 0;
        _titleBar.RelWidth = 1;
        _titleBar.RelHeight = TitleBarHeight;
        _titleBar.Align = Align.None;
        _titleBar.FillScale = FillScale.None;

        Content.RelX = 0;
        Content.RelY = TitleBarHeight;
        Content.RelWidth = 1;
        Content.RelHeight = 1 - TitleBarHeight;
        Content.Align = Align.None;
        Content.FillScale = FillScale.None;
    }

    /// <summary>Handles title-bar dragging; scrolling is owned by the content layer.</summary>
    protected override void Updater(InputHandler inputHandler)
    {
        base.Updater(inputHandler);

        if (!Draggable)
            return;

        var mousePos = Mouse.GetState().Position;

        // Drag only while the title bar holds the press capture (the press started on it).
        var dragging = _titleBar.IsMouseHold;
        if (dragging && !_dragging)
            _lastMouse = mousePos; // drag starts: no jump from a stale reference
        _dragging = dragging;

        if (dragging)
        {
            X = (X ?? 0) + (mousePos.X - _lastMouse.X) / UiScale;
            Y = (Y ?? 0) + (mousePos.Y - _lastMouse.Y) / UiScale;
            _lastMouse = mousePos;
        }
    }
}
