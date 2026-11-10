// UiScrollArea.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

/// <summary>
///     Scrollable container: children keep the slots assigned on <see cref="Add" />, and the mouse
///     wheel (while hovered) shifts them up within the clipped bounds. Composable — use it as a
///     tabview frame, a settings panel, a window content layer, or stand-alone. Nested areas scroll
///     innermost-first; the slot (<see cref="UiElement.RelY" />) is captured on Add and later
///     manual changes are overwritten while <see cref="ScrollOffset" /> is non-zero.
/// </summary>
public class UiScrollArea : UiContainer
{
    // Wheel value the innermost hovered scroll area last consumed; outer areas skip the same event.
    private static int _wheelConsumedAt;
    private readonly Dictionary<UiElement, float> _baseRelY = new();
    private int _lastWheel;

    /// <summary>Current scroll offset in (unscaled) pixels; content moves up by this amount.</summary>
    public float ScrollOffset { get; private set; }

    /// <summary>Pixels scrolled per mouse-wheel unit (wheel delta is ±120 per notch).</summary>
    public float ScrollSpeed = 0.1f;

    /// <summary>Appends a child and remembers its unscrolled slot.</summary>
    public override void Add(UiElement child)
    {
        base.Add(child);
        _baseRelY[child] = child.RelY;
    }

    /// <summary>Removes all children and their remembered slots.</summary>
    public override void Clear()
    {
        base.Clear();
        _baseRelY.Clear();
    }

    /// <summary>Shifts every child up by the scroll offset (fraction of the visible height).</summary>
    protected override void LayoutChildren()
    {
        var height = Math.Max(Bounds.Height, 1);
        foreach (var child in Children)
            if (_baseRelY.TryGetValue(child, out var baseRelY))
                child.RelY = baseRelY - ScrollOffset / height;
    }

    /// <summary>Updates children first (nested areas may consume the wheel), then scrolls on the wheel.</summary>
    protected override void Updater(InputHandler inputHandler)
    {
        base.Updater(inputHandler);
        PruneDisposed();

        var mouse = Mouse.GetState();
        var wheelDelta = mouse.ScrollWheelValue - _lastWheel;
        _lastWheel = mouse.ScrollWheelValue;
        if (wheelDelta == 0 || !Bounds.Contains(mouse.Position))
            return;

        var maxOffset = MaxScrollOffset();
        if (maxOffset <= 0 || _wheelConsumedAt == mouse.ScrollWheelValue)
            return; // nothing to scroll here, or an inner area already consumed this wheel event

        ScrollOffset = Math.Clamp(ScrollOffset - wheelDelta * ScrollSpeed, 0, maxOffset);
        _wheelConsumedAt = mouse.ScrollWheelValue;
    }

    /// <summary>Drops base slots of children the container purge already removed.</summary>
    private void PruneDisposed()
    {
        List<UiElement> disposed = null;
        foreach (var child in _baseRelY.Keys)
            if (child.IsDisposed)
                (disposed ??= new List<UiElement>()).Add(child);
        if (disposed is null)
            return;
        foreach (var child in disposed)
            _baseRelY.Remove(child);
    }

    /// <summary>Returns how far the children's bottoms overflow the visible area.</summary>
    private float MaxScrollOffset()
    {
        if (Bounds.Height <= 0)
            return 0;
        var maxBottom = 0f;
        foreach (var child in Children)
            if (_baseRelY.TryGetValue(child, out var baseRelY))
                maxBottom = Math.Max(maxBottom, baseRelY * Bounds.Height + child.Bounds.Height);
        return Math.Max(0, maxBottom - Bounds.Height);
    }
}
