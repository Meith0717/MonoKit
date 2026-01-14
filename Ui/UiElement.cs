// UiElement.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;
# if DEBUG
using MonoGame.Extended;
# endif

namespace MonoKit.Ui;

public enum Allign
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
    CenterV
}

public enum FillScale
{
    X,
    Y,
    Both,
    FillIn,
    Fit,
    None
}

public abstract class UiElement : IDisposable
{
    public Allign Allign = Allign.None;
    public FillScale FillScale = FillScale.None;
    public int? Height = null;

    public int? HSpace = null;
    public float RelHeight = .1f;
    public float RelWidth = .1f;

    public float RelX = 0;
    public float RelY = 0;
    public int? VSpace = null;

    public int? Width = null;
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
        var x = (int)float.Floor(root.Width * RelX);
        var y = (int)float.Floor(root.Height * RelY);

        var width = (int)float.Floor(Width * UiScale ?? root.Width * RelWidth);
        var height = (int)float.Floor(Height * UiScale ?? root.Height * RelHeight);

        ManageFillScale(root, FillScale, ref x, ref y, ref width, ref height);
        ManageAllign(root, Allign, ref x, ref y, ref width, ref height);
        ManageSpacing(root, ref x, ref y, ref width, ref height, HSpace.HasValue ? HSpace * UiScale : null,
            VSpace.HasValue ? VSpace * UiScale : null);

        Bounds = new Rectangle(root.X + x, root.Y + y, width, height);
    }

    private static void ManageFillScale(Rectangle root, FillScale fillScale, ref int x, ref int y, ref int width,
        ref int height)
    {
        if (fillScale == FillScale.None) return;

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

    private static void ManageAllign(Rectangle root, Allign allign, ref int x, ref int y, ref int width, ref int height)
    {
        x = allign switch
        {
            Allign.NW => 0,
            Allign.SW => 0,
            Allign.W => 0,
            Allign.Left => 0,
            Allign.N => (root.Width - width) / 2,
            Allign.Center => (root.Width - width) / 2,
            Allign.CenterV => (root.Width - width) / 2,
            Allign.S => (root.Width - width) / 2,
            Allign.NE => root.Width - width,
            Allign.E => root.Width - width,
            Allign.Right => root.Width - width,
            Allign.SE => root.Width - width,
            Allign.CenterH => x,
            Allign.None => x,
            Allign.Bottom => x,
            Allign.Top => x,
            _ => throw new NotImplementedException()
        };
        y = allign switch
        {
            Allign.NW => 0,
            Allign.N => 0,
            Allign.NE => 0,
            Allign.Top => 0,
            Allign.E => (root.Height - height) / 2,
            Allign.W => (root.Height - height) / 2,
            Allign.Center => (root.Height - height) / 2,
            Allign.CenterH => (root.Height - height) / 2,
            Allign.SE => root.Height - height,
            Allign.S => root.Height - height,
            Allign.Bottom => root.Height - height,
            Allign.SW => root.Height - height,
            Allign.CenterV => y,
            Allign.None => y,
            Allign.Left => y,
            Allign.Right => y,
            _ => throw new NotImplementedException()
        };
    }

    private static void ManageSpacing(Rectangle root, ref int x, ref int y, ref int width, ref int height,
        float? hSpace, float? vSpace)
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
                if (spaceLeft < hSpace) x = 0 + (int)hSpace;
                if (spaceRight < hSpace) x -= (int)hSpace;
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
                if (spaceTop < vSpace) y = 0 + (int)vSpace;
                if (spaceBottom < vSpace) y -= (int)vSpace;
            }
        }
    }
}