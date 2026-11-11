// MonoKit - UiElement.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoKit.App.Input;

namespace MonoKit.App.Ui;

public enum Align
{
    N,
    NE,
    E,
    SE,
    S,
    SW,
    W,
    NW,
    Center,
    None,
    CenterH,
    CenterV,
}

public enum FillScale
{
    X,
    Y,
    Both,
    FillIn,
    Fit,
    None,
}

public abstract class UiElement : IDisposable
{
    public Align Align = Align.None;

    /// <summary>Ancestor clip rectangle (set by clipping containers during Draw); null = no clip.</summary>
    internal Rectangle? ClipRect;

    public FillScale FillScale = FillScale.None;

    public int? HSpace = null;
    protected int? IntrinsicHeight;

    /// <summary>
    ///     Content-derived size, computed by <see cref="MeasureSize" />.
    ///     Unlike <see cref="Width" />/<see cref="Height" />, a user-assigned explicit size always wins.
    /// </summary>
    protected int? IntrinsicWidth;

    public float RelHeight = .1f;
    public float RelWidth = .1f;

    public float RelX = 0;
    public float RelY = 0;
    public int? VSpace = null;

    public float? X = null;
    public float? Y = null;
    private bool _pressStartedHere;

    private ButtonState _previousLeft;

    /// <summary>Explicit height in pixels; when null, derived from <see cref="RelHeight" /> or intrinsic size.</summary>
    public int? Height { get; set; }

    /// <summary>Explicit width in pixels; when null, derived from <see cref="RelWidth" /> or intrinsic size.</summary>
    public int? Width { get; set; }

    public Rectangle Bounds { get; private set; }
    public bool IsDisposed { get; private set; }
    protected float UiScale { get; private set; }

    /// <summary>True on the frame the left button goes down over this element.</summary>
    internal bool IsMousePressed { get; private set; }

    /// <summary>True while the left button is held and the press started on this element.</summary>
    internal bool IsMouseHold { get; private set; }

    /// <summary>True on the frame the left button, pressed on this element, is released over it.</summary>
    internal bool IsMouseClicked { get; private set; }

    /// <summary>Marks the element disposed; containers purge disposed children.</summary>
    public virtual void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>Hook: performs the element’s per-frame update and input handling.</summary>
    protected abstract void Updater(InputHandler inputHandler);

    /// <summary>Hook: draws the element’s own content.</summary>
    protected abstract void Drawer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice);

    /// <summary>
    ///     Computes the element's <see cref="IntrinsicWidth" />/<see cref="IntrinsicHeight" /> before layout.
    ///     Override in widgets whose size derives from content (text, texture).
    /// </summary>
    protected virtual void MeasureSize() { }

    /// <summary>Relayouts this element, tracks the mouse press, then updates it unless clipped away.</summary>
    public void Update(InputHandler inputHandler, Rectangle root, float uiScale)
    {
        ApplyScale(root, uiScale);
        var visible = ClipRect is not { } clip || Bounds.Intersects(clip);
        TrackMousePress(visible);
        if (!visible)
            return; // clipped away: no input either
        Updater(inputHandler);
    }

    /// <summary>
    ///     Updates the press-capture state. An element only reacts to a held or released button
    ///     when the press started on it, so pressing elsewhere and then hovering never triggers it.
    /// </summary>
    private void TrackMousePress(bool visible)
    {
        var mouse = Mouse.GetState();
        var downEdge =
            mouse.LeftButton == ButtonState.Pressed && _previousLeft == ButtonState.Released;
        var upEdge =
            mouse.LeftButton == ButtonState.Released && _previousLeft == ButtonState.Pressed;
        var hovered = visible && Bounds.Contains(mouse.Position);

        if (downEdge)
            _pressStartedHere = hovered;
        IsMousePressed = downEdge && _pressStartedHere;
        IsMouseHold = mouse.LeftButton == ButtonState.Pressed && _pressStartedHere && visible;
        IsMouseClicked = upEdge && _pressStartedHere && hovered;
        if (upEdge)
            _pressStartedHere = false;
        _previousLeft = mouse.LeftButton;
    }

    /// <summary>Draws the element unless an ancestor clip rect culls it.</summary>
    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        if (ClipRect is { } clip && !Bounds.Intersects(clip))
            return; // culled by an ancestor's clip rect
        Drawer(spriteBatch, graphicsDevice);
#if DEBUG
        spriteBatch.DrawRectangle(Bounds, Color.MonoGameOrange);
#endif
    }

    /// <summary>Recomputes layout from the given root rectangle and UI scale.</summary>
    public virtual void ApplyScale(Rectangle root, float uiScale)
    {
        UiScale = uiScale;
        MeasureSize();
        UpdateBounds(root);
    }

    /// <summary>Computes bounds from position, size, fill, align and spacing settings.</summary>
    private void UpdateBounds(Rectangle root)
    {
        var x = (int)float.Floor(X * UiScale ?? root.Width * RelX);
        var y = (int)float.Floor(Y * UiScale ?? root.Height * RelY);

        // Size priority: explicit Width/Height, then measured intrinsic size, then relative fractions.
        var width = (int)float.Floor((Width ?? IntrinsicWidth) * UiScale ?? root.Width * RelWidth);
        var height = (int)
            float.Floor((Height ?? IntrinsicHeight) * UiScale ?? root.Height * RelHeight);

        ManageFillScale(root, FillScale, ref x, ref y, ref width, ref height);
        ManageAlign(root, Align, ref x, ref y, ref width, ref height);
        ManageSpacing(
            root,
            ref x,
            ref y,
            ref width,
            ref height,
            HSpace * UiScale,
            VSpace * UiScale
        );

        Bounds = new Rectangle(root.X + x, root.Y + y, width, height);
    }

    /// <summary>Adjusts size per fill mode: stretch or fit within the root.</summary>
    private static void ManageFillScale(
        Rectangle root,
        FillScale fillScale,
        ref int x,
        ref int y,
        ref int width,
        ref int height
    )
    {
        if (fillScale == FillScale.None)
            return;

        var rootAspectRatio = root.Width / (float)root.Height;
        var aspectRatio = width / (float)height;

        switch (fillScale)
        {
            case FillScale.X:
                width = root.Width;
                height = (int)(width / aspectRatio);
                break;
            case FillScale.Y:
                height = root.Height;
                width = (int)(height * aspectRatio);
                break;
            case FillScale.Both:
                x = 0;
                y = 0;
                height = root.Height;
                width = root.Width;
                break;
            case FillScale.FillIn:
                if (aspectRatio > rootAspectRatio)
                {
                    height = root.Height;
                    width = (int)(height * aspectRatio);
                }

                if (aspectRatio < rootAspectRatio)
                {
                    width = root.Width;
                    height = (int)(width / aspectRatio);
                }

                if (aspectRatio == rootAspectRatio)
                {
                    x = 0;
                    y = 0;
                    height = root.Height;
                    width = root.Width;
                }

                break;
            case FillScale.Fit:
                if (aspectRatio > rootAspectRatio)
                {
                    width = root.Width;
                    height = (int)(width / aspectRatio);
                }

                if (aspectRatio < rootAspectRatio)
                {
                    height = root.Height;
                    width = (int)(height * aspectRatio);
                }

                if (aspectRatio == rootAspectRatio)
                {
                    x = 0;
                    y = 0;
                    height = root.Height;
                    width = root.Width;
                }

                break;
        }
    }

    /// <summary>Positions the element within the root according to alignment.</summary>
    private static void ManageAlign(
        Rectangle root,
        Align align,
        ref int x,
        ref int y,
        ref int width,
        ref int height
    )
    {
        x = align switch
        {
            Align.NW => 0,
            Align.SW => 0,
            Align.W => 0,
            Align.N => (root.Width - width) / 2,
            Align.Center => (root.Width - width) / 2,
            Align.CenterH => (root.Width - width) / 2,
            Align.S => (root.Width - width) / 2,
            Align.NE => root.Width - width,
            Align.E => root.Width - width,
            Align.SE => root.Width - width,
            Align.CenterV => x,
            Align.None => x,
            _ => throw new NotImplementedException(),
        };
        y = align switch
        {
            Align.NW => 0,
            Align.N => 0,
            Align.NE => 0,
            Align.E => (root.Height - height) / 2,
            Align.W => (root.Height - height) / 2,
            Align.Center => (root.Height - height) / 2,
            Align.CenterV => (root.Height - height) / 2,
            Align.SE => root.Height - height,
            Align.S => root.Height - height,
            Align.SW => root.Height - height,
            Align.CenterH => y,
            Align.None => y,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>Applies HSpace and VSpace margins within the root.</summary>
    private static void ManageSpacing(
        Rectangle root,
        ref int x,
        ref int y,
        ref int width,
        ref int height,
        float? hSpace,
        float? vSpace
    )
    {
        if (hSpace != null)
        {
            var spaceLeft = x;
            var spaceRight = root.Width - (width + x);

            if (spaceLeft < hSpace && spaceRight < hSpace)
            {
                x += (int)hSpace;
                width -= (int)hSpace * 2;
            }
            else
            {
                if (spaceLeft < hSpace)
                    x = 0 + (int)hSpace;
                if (spaceRight < hSpace)
                    x -= (int)hSpace;
            }
        }

        if (vSpace != null)
        {
            var spaceTop = y;
            var spaceBottom = root.Height - (height + y);

            if (spaceTop < vSpace && spaceBottom < vSpace)
            {
                y += (int)vSpace;
                height -= (int)vSpace * 2;
            }
            else
            {
                if (spaceTop < vSpace)
                    y = 0 + (int)vSpace;
                if (spaceBottom < vSpace)
                    y -= (int)vSpace;
            }
        }
    }
}
