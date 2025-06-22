// Renderer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Engine.Content;
using Engine.Extensions;
using Engine.Gameplay;
using Engine.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.Camera
{
    internal class Renderer()
    {
        private List<GameObject> _objects = new();
        private readonly Texture2D _sphere = ContentProvider.Textures.Get("sphere");
        private readonly Texture2D _starLight = ContentProvider.Textures.Get("star_light");
        private readonly Texture2D _planetShadow = ContentProvider.Textures.Get("planet_shadow");

        public void FrustumCulling(RectangleF viewFrustum, SpatialHashing spatialHashing)
        {
            _objects.Clear();
            spatialHashing.GetObjectsInRectangle(viewFrustum, ref _objects);
        }

        public void RenderCulledObjects(SpriteBatch spriteBatch, Matrix matrix, GameRuntime runtime)
        {
            var font = ContentProvider.Fonts.Get("defaultFont");
            foreach (GameObject obj in _objects)
            {
                if (Debugger.IsAttached)
                {
                    spriteBatch.Begin(transformMatrix: matrix);
                    spriteBatch.DrawCircleF(obj.BoundBox.Position, obj.BoundBox.Radius, Color.Purple, .5f * runtime.Camera.Zoom);
                    spriteBatch.DrawLine(obj.Position, obj.Position.InDirection(obj.MovingDirection, obj.Velocity * 500), Color.Blue, .5f * runtime.Camera.Zoom, 1);
                    spriteBatch.DrawString(font, $"{obj.GetType().Name}", obj.BoundBox.ToRectangleF().TopLeft, Color.Purple, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 1);
                    spriteBatch.End();
                }

                spriteBatch.Begin(transformMatrix: matrix);
                spriteBatch.DrawGameObject(obj);
                spriteBatch.End();
            }
        }
    }
}
