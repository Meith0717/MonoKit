// UiContainer.cs
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

/// <summary>
///     Concrete container: owns the child list, cascades update/draw/scale/dispose, and lets
///     subclasses define the child layout via <see cref="LayoutChildren" />.
///     Draws an optional background — a solid <see cref="Color" /> or an attached texture
///     (transparent by default: no background).
///     When <see cref="ClipChildren" /> is true, the container's bounds become the clip rect for
///     all descendants: culled on the CPU (draw + input) and scissored on the GPU.
/// </summary>
public class UiContainer : UiElement
{
    private readonly List<UiElement> _children = new();
    private Color _color = Color.Transparent;
    private Texture2D _backgroundTexture;
    private float _textureScale = 1;

    public IReadOnlyList<UiElement> Children => _children;

    /// <summary>Background color or texture tint; <see cref="Color.Transparent" /> draws no solid background.</summary>
    public Color Color
    {
        get => _color;
        set => _color = value;
    }

    /// <summary>
    ///     Clip all descendants to this container's <see cref="UiElement.Bounds" /> (GPU scissor + CPU cull).
    ///     Default on for a WYSIWYG layout; set false to let children overflow (tooltips, dropdowns).
    /// </summary>
    public bool ClipChildren { get; set; } = true;

    public virtual void Add(UiElement child) => _children.Add(child);

    /// <summary>Removes all children without disposing them (Dispose cascades disposal).</summary>
    public virtual void Clear() => _children.Clear();

    /// <summary>Assigns relative layout slots to children; called after the container's own bounds are computed.</summary>
    protected virtual void LayoutChildren() { }

    /// <summary>Uses <paramref name="texture" /> as the background (stretched to the bounds).</summary>
    public UiContainer AttachTexture(Texture2D texture, float scale = 1)
    {
        _backgroundTexture = texture;
        _textureScale = scale;
        // An attached background texture is meant to be visible; don't keep a transparent tint.
        if (_color == Color.Transparent)
            _color = Color.White;
        return this;
    }

    protected override void MeasureSize()
    {
        if (_backgroundTexture is null)
            return;

        IntrinsicWidth = (int)(_backgroundTexture.Width * _textureScale);
        IntrinsicHeight = (int)(_backgroundTexture.Height * _textureScale);
    }

    protected override void Updater(InputHandler inputHandler)
    {
        _children.RemoveAll(c => c.IsDisposed);
        foreach (var child in _children)
            child.Update(inputHandler, Bounds, UiScale);
    }

    protected override void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        if (_backgroundTexture is not null)
            spriteBatch.Draw(_backgroundTexture, Bounds, _color);
        else if (_color != Color.Transparent)
            spriteBatch.FillRectangle(Bounds, _color);

        if (!ClipChildren)
        {
            foreach (var child in _children)
                child.Draw(spriteBatch, graphicsDevice);
            return;
        }

        // Clip this container's subtree: GPU scissor rectangle (children are additionally
        // culled on the CPU via ClipRect during Draw/Update).
        var previousScissor = graphicsDevice.ScissorRectangle;
        graphicsDevice.ScissorRectangle = Rectangle.Intersect(previousScissor, Bounds);
        foreach (var child in _children)
            child.Draw(spriteBatch, graphicsDevice);
        graphicsDevice.ScissorRectangle = previousScissor;
    }

    public override void ApplyScale(Rectangle root, float uiScale)
    {
        base.ApplyScale(root, uiScale);
        LayoutChildren();

        // Cascade the clip rect down during layout so both Update (input) and Draw see it.
        var childClip = ClipChildren ? Rectangle.Intersect(ClipRect ?? Bounds, Bounds) : ClipRect;
        foreach (var child in _children)
        {
            child.ClipRect = childClip;
            child.ApplyScale(Bounds, uiScale);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        foreach (var child in _children)
            child.Dispose();
    }
}
