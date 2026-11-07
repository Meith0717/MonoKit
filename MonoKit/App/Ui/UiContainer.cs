// AppHost - UiContainer.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

/// <summary>
///     Base class for elements with children: owns the child list, cascades update/draw/scale/dispose,
///     and lets subclasses define the child layout via <see cref="LayoutChildren" />.
/// </summary>
public abstract class UiContainer : UiElement
{
    private readonly List<UiElement> _children = new();

    public IReadOnlyList<UiElement> Children => _children;

    public void Add(UiElement child) => _children.Add(child);

    /// <summary>Removes all children without disposing them (Dispose cascades disposal).</summary>
    public void Clear() => _children.Clear();

    /// <summary>Assigns relative layout slots to children; called after the container's own bounds are computed.</summary>
    protected virtual void LayoutChildren() { }

    protected virtual void DrawBackground(SpriteBatch spriteBatch) { }

    protected override void Updater(InputHandler inputHandler)
    {
        _children.RemoveAll(c => c.IsDisposed);
        foreach (var child in _children)
            child.Update(inputHandler, Bounds, UiScale);
    }

    protected override void Drawer(SpriteBatch spriteBatch)
    {
        DrawBackground(spriteBatch);
        foreach (var child in _children)
            child.Draw(spriteBatch);
    }

    public override void ApplyScale(Rectangle root, float uiScale)
    {
        base.ApplyScale(root, uiScale);
        LayoutChildren();
        foreach (var child in _children)
            child.ApplyScale(Bounds, uiScale);
    }

    public override void Dispose()
    {
        base.Dispose();
        foreach (var child in _children)
            child.Dispose();
    }
}
