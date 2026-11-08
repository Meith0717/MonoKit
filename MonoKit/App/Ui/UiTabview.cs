// AppHost - UiTabView.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

public enum TabAxis : byte
{
    X,
    Y,
}

/// <summary>
///     Tabbed view composed from a <see cref="UiHBox" /> button bar and a content <see cref="UiFrame" />;
///     the active tab's frame is shown in the content area.
/// </summary>
public sealed class UiTabView<ButtonType>(Color color) : UiElement
    where ButtonType : UiElement, IButton
{
    private readonly UiHBox _buttonBar = new();
    private readonly UiFrame _contentFrame = new() { Color = Color.Transparent };
    private readonly List<(ButtonType Button, UiFrame Frame)> _tabs = new();
    private UiFrame _activeFrame;

    /// <summary>Highlight color applied to the active tab's button.</summary>
    public Color ActiveTabColor { get; set; } = Color.Red;

    public void Add(ButtonType button, UiFrame tabFrame)
    {
        button.OnClickAction += () => _activeFrame = tabFrame;
        _buttonBar.Add(button);
        _tabs.Add((button, tabFrame));
    }

    /// <summary>Positions the button bar and content area. Axis X = bar on top, Y = bar on the left.</summary>
    public void Initialize(TabAxis axis, float tabButtonRatio)
    {
        if (axis == TabAxis.X)
        {
            _buttonBar.RelX = 0;
            _buttonBar.RelY = 0;
            _buttonBar.RelWidth = 1;
            _buttonBar.RelHeight = tabButtonRatio;

            _contentFrame.RelX = 0;
            _contentFrame.RelY = tabButtonRatio;
            _contentFrame.RelWidth = 1;
            _contentFrame.RelHeight = 1 - tabButtonRatio;
        }
        else
        {
            _buttonBar.RelX = 0;
            _buttonBar.RelY = 0;
            _buttonBar.RelWidth = tabButtonRatio;
            _buttonBar.RelHeight = 1;

            _contentFrame.RelX = tabButtonRatio;
            _contentFrame.RelY = 0;
            _contentFrame.RelWidth = 1 - tabButtonRatio;
            _contentFrame.RelHeight = 1;
        }

        if (_activeFrame == null && _tabs.Count > 0)
            _activeFrame = _tabs[0].Frame;
    }

    protected override void Updater(InputHandler inputHandler)
    {
        _buttonBar.Update(inputHandler, Bounds, UiScale);

        foreach (var (button, frame) in _tabs)
            if (_activeFrame == frame)
                button.OverrideColor(ActiveTabColor, ActiveTabColor);

        _contentFrame.Clear();
        if (_activeFrame != null)
            _contentFrame.Add(_activeFrame);
        _contentFrame.Update(inputHandler, Bounds, UiScale);
    }

    protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        spriteBatch.FillRectangle(Bounds, color);
        _contentFrame.Draw(spriteBatch, graphicsDevice);
        _buttonBar.Draw(spriteBatch, graphicsDevice);
    }

    public override void ApplyScale(Rectangle root, float uiScale)
    {
        base.ApplyScale(root, uiScale);
        _buttonBar.ApplyScale(Bounds, uiScale);
        _contentFrame.ApplyScale(Bounds, uiScale);
    }

    public override void Dispose()
    {
        base.Dispose();
        _buttonBar.Dispose();
        _contentFrame.Dispose();
    }
}
