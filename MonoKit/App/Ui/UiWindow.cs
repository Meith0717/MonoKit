// UiWindow.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

/// <summary>
/// Draggable, scrollable window: a title bar on top and a clipped content layer below.
/// Dragging uses the title bar (hold left mouse), scrolling uses the wheel while hovered.
/// Child rendering is clipped to the content bounds via the GPU scissor test.
/// </summary>
public class UiWindow : UiContainer
{
    private readonly UiContainer _titleBar;
    private readonly UiText _titleText;
    private readonly UiContainer _content;
    private readonly List<float> _contentBaseRelY = [];
    private bool _dragging;
    private Point _lastMouse;
    private int _lastWheel;

    /// <summary>Builds the title bar with its text and the content layer.</summary>
    public UiWindow(SpriteFont font, string title)
    {
        _titleText = new UiText(font, title) { Align = Align.W, Color = Color.White };
        _titleBar = new UiContainer { Color = new Color(40, 40, 40, 220) };
        _titleBar.Add(_titleText);

        _content = new UiContainer();

        Add(_titleBar);
        Add(_content);
    }

    public string Title
    {
        get => _titleText.Text;
        set => _titleText.Text = value;
    }

    /// <summary>Title bar height as a fraction of the window height.</summary>
    public float TitleBarHeight = 0.12f;

    public bool Draggable = true;

    /// <summary>Current scroll offset in (unscaled) pixels; content moves up by this amount.</summary>
    public float ScrollOffset { get; private set; }

    /// <summary>Pixels scrolled per mouse-wheel unit (wheel delta is ±120 per notch).</summary>
    public float ScrollSpeed = 0.1f;

    public Color TitleBarColor
    {
        get => _titleBar.Color;
        set => _titleBar.Color = value;
    }

    /// <summary>The scrollable content layer (children render clipped to its bounds).</summary>
    public UiContainer Content => _content;


    /// <summary>Adds a child to the scrollable content layer.</summary>
    public void AddContent(UiElement child)
    {
        _content.Add(child);
        _contentBaseRelY.Add(child.RelY);
    }

    /// <summary>Positions the title bar and the scrolled content layer.</summary>
    protected override void LayoutChildren()
    {
        _titleBar.RelX = 0;
        _titleBar.RelY = 0;
        _titleBar.RelWidth = 1;
        _titleBar.RelHeight = TitleBarHeight;
        _titleBar.Align = Align.None;
        _titleBar.FillScale = FillScale.None;

        _content.RelX = 0;
        _content.RelY = TitleBarHeight;
        _content.RelWidth = 1;
        _content.RelHeight = 1 - TitleBarHeight;
        _content.Align = Align.None;
        _content.FillScale = FillScale.None;

        // Apply scroll: shift content children up by ScrollOffset (in fraction of the content height).
        var children = _content.Children;
        var contentHeight = Math.Max(Bounds.Height * (1 - TitleBarHeight), 1);
        for (var i = 0; i < children.Count; i++)
            children[i].RelY = _contentBaseRelY[i] - ScrollOffset / contentHeight;
    }

    /// <summary>Handles title-bar dragging and mouse-wheel scrolling.</summary>
    protected override void Updater(InputHandler inputHandler)
    {
        base.Updater(inputHandler);

        var mouse = Mouse.GetState();
        var mousePos = mouse.Position;

        if (Draggable)
        {
            var leftDown = mouse.LeftButton == ButtonState.Pressed;
            if (!_dragging && leftDown && _titleBar.Bounds.Contains(mousePos))
            {
                _dragging = true;
                _lastMouse = mousePos;
            }
            else if (_dragging && !leftDown)
            {
                _dragging = false;
            }

            if (_dragging)
            {
                X = (X ?? 0) + (mousePos.X - _lastMouse.X) / UiScale;
                Y = (Y ?? 0) + (mousePos.Y - _lastMouse.Y) / UiScale;
                _lastMouse = mousePos;
            }
        }

        var wheelDelta = mouse.ScrollWheelValue - _lastWheel;
        _lastWheel = mouse.ScrollWheelValue;
        if (Bounds.Contains(mousePos) && wheelDelta != 0)
            ScrollOffset = Math.Clamp(ScrollOffset - wheelDelta * ScrollSpeed, 0, MaxScrollOffset());
    }

    /// <summary>Returns the content height overflowing the visible area.</summary>
    private float MaxScrollOffset()
    {
        var contentHeight = _content.Bounds.Height;
        if (contentHeight <= 0)
            return 0;
        var children = _content.Children;
        float maxBottom = 0;
        for (var i = 0; i < children.Count; i++)
        {
            var childTopPx = _contentBaseRelY[i] * contentHeight;
            maxBottom = Math.Max(maxBottom, childTopPx + children[i].Bounds.Height);
        }
        return Math.Max(0, maxBottom - contentHeight);
    }
}
