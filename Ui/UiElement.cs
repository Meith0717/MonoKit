// UiElement.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Diagnostics;

namespace GameEngine.Ui
{
    public enum Anchor { N, NE, E, SE, S, SW, W, NW, Center, None, Left, Right, Top, Bottom, CenterH, CenterV }

    public enum FillScale { X, Y, Both, FillIn, Fit, None }

    public abstract class UiElement : IDisposable
    {
        public Rectangle Bounds { get; private set; } = new();
        public bool IsDisposed { get; private set; }
        protected float UiScale { get; private set; }

        public int? X = null;
        public int? Y = null;
        public float RelX = 0;
        public float RelY = 0;

        public int? Width = null;
        public int? Height = null;
        public float RelWidth = .1f;
        public float RelHeight = .1f;

        public int? HSpace = null;
        public int? VSpace = null;

        public Anchor Anchor = Anchor.None;
        public FillScale FillScale = FillScale.None;

        protected abstract void Updater(InputState inputState);

        protected abstract void Drawer(SpriteBatch spriteBatch);

        public void Update(InputState inputState, Rectangle root, float uiScale)
        {
            ApplyScale(root, uiScale);
            Updater(inputState);
            ApplyScale(root, uiScale);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Drawer(spriteBatch);
            if (Debugger.IsAttached)
            {
                spriteBatch.DrawRectangle(Bounds, Color.Red, 1);
            }
        }

        public virtual void ApplyScale(Rectangle root, float uiScale)
        {
            UiScale = uiScale;
            UpdateBounds(root);
        }

        public virtual void Dispose()
        {
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        private void UpdateBounds(Rectangle root)
        {
            int x = X ?? (int)float.Floor(root.Width * RelX);
            int y = Y ?? (int)float.Floor(root.Height * RelY);

            int width = (int)float.Floor(Width * UiScale ?? root.Width * RelWidth);
            int height = (int)float.Floor(Height * UiScale ?? root.Height * RelHeight);

            ManageFillScale(root, FillScale, ref x, ref y, ref width, ref height);
            ManageAnchor(root, Anchor, ref x, ref y, ref width, ref height);
            ManageSpacing(root, ref x, ref y, ref width, ref height, HSpace.HasValue ? HSpace * UiScale : null, VSpace.HasValue ? VSpace * UiScale : null);

            Bounds = new(root.X + x, root.Y + y, width, height);
        }

        private static void ManageFillScale(Rectangle root, FillScale fillScale, ref int x, ref int y, ref int width, ref int height)
        {
            if (fillScale == FillScale.None) return;

            float rootAspectRatio = root.Width / (float)root.Height;
            float aspectRatio = width / (float)height;

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
                    x = 0; y = 0;
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
                        x = 0; y = 0;
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
                        x = 0; y = 0;
                        height = root.Height;
                        width = root.Width;
                    }
                    break;
            }
        }

        private static void ManageAnchor(Rectangle root, Anchor anchor, ref int x, ref int y, ref int width, ref int height)
        {
            x = anchor switch
            {
                Anchor.NW => 0,
                Anchor.SW => 0,
                Anchor.W => 0,
                Anchor.Left => 0,
                Anchor.N => (root.Width - width) / 2,
                Anchor.Center => (root.Width - width) / 2,
                Anchor.CenterV => (root.Width - width) / 2,
                Anchor.S => (root.Width - width) / 2,
                Anchor.NE => root.Width - width,
                Anchor.E => root.Width - width,
                Anchor.Right => root.Width - width,
                Anchor.SE => root.Width - width,
                Anchor.CenterH => x,
                Anchor.None => x,
                Anchor.Bottom => x,
                Anchor.Top => x,
                _ => throw new System.NotImplementedException()
            };
            y = anchor switch
            {
                Anchor.NW => 0,
                Anchor.N => 0,
                Anchor.NE => 0,
                Anchor.Top => 0,
                Anchor.E => (root.Height - height) / 2,
                Anchor.W => (root.Height - height) / 2,
                Anchor.Center => (root.Height - height) / 2,
                Anchor.CenterH => (root.Height - height) / 2,
                Anchor.SE => root.Height - height,
                Anchor.S => root.Height - height,
                Anchor.Bottom => root.Height - height,
                Anchor.SW => root.Height - height,
                Anchor.CenterV => y,
                Anchor.None => y,
                Anchor.Left => y,
                Anchor.Right => y,
                _ => throw new System.NotImplementedException()
            };
        }

        private static void ManageSpacing(Rectangle root, ref int x, ref int y, ref int width, ref int height, float? hSpace, float? vSpace)
        {
            if (hSpace != null)
            {
                int spaceLeft = x;
                int spaceRight = root.Width - (width + x);

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
                int spaceTop = y;
                int spaceBottom = root.Height - (height + y);

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
}
