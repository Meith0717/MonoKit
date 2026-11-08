// AppHost - UiTabView.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

public enum TabAxis : byte
{
    X,
    Y,
}

/// <summary>
///     Tabbed view composed from a <see cref="UiHBox" /> button bar and a content <see cref="UiContainer" />;
///     the active tab's frame is shown in the content area. Being a plain container,
///     the whole subtree (background, clipping) is managed by the standard cascade.
/// </summary>
public sealed class UiTabView<ButtonType> : UiContainer
    where ButtonType : UiElement, IButton
{
    private readonly UiHBox _buttonBar = new();
    private readonly UiContainer _contentFrame = new();
    private readonly List<(ButtonType Button, UiContainer Frame)> _tabs = new();
    private UiContainer _activeFrame;

    /// <summary>Highlight color applied to the active tab's button.</summary>
    public Color ActiveTabColor { get; set; } = Color.Red;

    public UiTabView(Color color)
    {
        Color = color;
        Add(_buttonBar);
        Add(_contentFrame);
    }

    public void Add(ButtonType button, UiContainer tabFrame)
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

    // The bar and content area manage their own slots (assigned in Initialize).
    protected override void LayoutChildren() { }

    protected override void Updater(InputHandler inputHandler)
    {
        // Swap in the active tab's frame so the child cascade below updates it this frame.
        _contentFrame.Clear();
        if (_activeFrame is { IsDisposed: false })
        {
            // The active frame fills the content area; the tab view owns this layout.
            _activeFrame.RelX = 0;
            _activeFrame.RelY = 0;
            _activeFrame.RelWidth = 1;
            _activeFrame.RelHeight = 1;
            _activeFrame.Align = Align.None;
            _activeFrame.FillScale = FillScale.None;
            _contentFrame.Add(_activeFrame);
        }

        base.Updater(inputHandler);

        foreach (var (button, frame) in _tabs)
        {
            if (_activeFrame == frame)
                button.OverrideColor(ActiveTabColor, ActiveTabColor);
            else
                button.ResetColor();
        }
    }
}
