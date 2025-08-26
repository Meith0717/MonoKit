// Renderer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Camera;
using GameEngine.Content;
using GameEngine.Extensions;
using GameEngine.Gameplay;
using GameEngine.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.Rendering
{
    internal class Renderer()
    {
        private List<GameObject> _objects = new();

        public void CullingObjects(RectangleF viewFrustum, SpatialHashing spatialHashing)
        {
            _objects.Clear();
            spatialHashing.GetObjectsInRectangle(viewFrustum, ref _objects);
        }

        public void Draw(SpriteBatch spriteBatch, Camera2d camera, RuntimeServiceContainer serviceContainer)
        {
            var font = ContentProvider.Fonts.Get("default_font");

            foreach (GameObject obj in _objects)
            {
                if (Debugger.IsAttached)
                {
                    spriteBatch.DrawCircleF(obj.BoundBox.Position, obj.BoundBox.Radius, Color.Purple, .5f * camera.Zoom);
                    spriteBatch.DrawLine(obj.Position, obj.Position.InDirection(obj.MovingDirection, obj.Velocity * 500), Color.Blue, 2f / camera.Zoom, 1);
                    spriteBatch.DrawString(font, $"{obj.GetType().Name}", obj.BoundBox.ToRectangleF().TopLeft, Color.Purple, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 1);
                }

                obj.Draw(spriteBatch, serviceContainer);
            }
        }
    }
}
