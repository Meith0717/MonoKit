// UiElement.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;
# if DEBUG
using MonoGame.Extended;
# endif

namespace MonoKit.Ui;

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
    Left,
    Right,
    Top,
    Bottom,
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
    public FillScale FillScale = FillScale.None;

    public int? HSpace = null;
    public int? VSpace = null;

    public float RelHeight = .1f;
    public float RelWidth = .1f;
    public int? Width = null;
    public int? Height = null;

    public float RelX = 0;
    public float RelY = 0;
    public float? X = null;
    public float? Y = null;

    public Rectangle Bounds { get; private set; }
    public bool IsDisposed { get; private set; }
    protected float UiScale { get; private set; }

    public virtual void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    protected abstract void Updater(InputHandler inputHandler);

    protected abstract void Drawer(SpriteBatch spriteBatch);

    public void Update(InputHandler inputHandler, Rectangle root, float uiScale)
    {
        ApplyScale(root, uiScale);
        Updater(inputHandler);
        ApplyScale(root, uiScale);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Drawer(spriteBatch);
# if DEBUG
        spriteBatch.DrawRectangle(Bounds, Color.DeepSkyBlue);
#endif
    }

    public virtual void ApplyScale(Rectangle root, float uiScale)
    {
        UiScale = uiScale;
        UpdateBounds(root);
    }

    private void UpdateBounds(Rectangle root)
    {
        var x = (int)float.Floor(X * UiScale ?? root.Width * RelX);
        var y = (int)float.Floor(Y * UiScale ?? root.Height * RelY);

        var width = (int)float.Floor(Width * UiScale ?? root.Width * RelWidth);
        var height = (int)float.Floor(Height * UiScale ?? root.Height * RelHeight);

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
            Align.Left => 0,
            Align.N => (root.Width - width) / 2,
            Align.Center => (root.Width - width) / 2,
            Align.CenterV => (root.Width - width) / 2,
            Align.S => (root.Width - width) / 2,
            Align.NE => root.Width - width,
            Align.E => root.Width - width,
            Align.Right => root.Width - width,
            Align.SE => root.Width - width,
            Align.CenterH => x,
            Align.None => x,
            Align.Bottom => x,
            Align.Top => x,
            _ => throw new NotImplementedException(),
        };
        y = align switch
        {
            Align.NW => 0,
            Align.N => 0,
            Align.NE => 0,
            Align.Top => 0,
            Align.E => (root.Height - height) / 2,
            Align.W => (root.Height - height) / 2,
            Align.Center => (root.Height - height) / 2,
            Align.CenterH => (root.Height - height) / 2,
            Align.SE => root.Height - height,
            Align.S => root.Height - height,
            Align.Bottom => root.Height - height,
            Align.SW => root.Height - height,
            Align.CenterV => y,
            Align.None => y,
            Align.Left => y,
            Align.Right => y,
            _ => throw new NotImplementedException(),
        };
    }

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
