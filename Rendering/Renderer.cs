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

namespace GameEngine.Rendering
{
    internal class Renderer()
    {
        private List<GameObject> _objects = new();
#if DEBUG
        private readonly static SpriteFont _font = ContentProvider.Fonts.Get("default_font");
#endif

        public void CullingObjects(RectangleF viewFrustum, SpatialHashing spatialHashing)
        {
            _objects.Clear();
            spatialHashing.GetObjectsInRectangle(viewFrustum, ref _objects);
        }

        public void Draw(SpriteBatch spriteBatch, Camera2d camera, RuntimeServiceContainer serviceContainer)
        {
            foreach (GameObject obj in _objects)
                obj.Draw(spriteBatch, serviceContainer);

#if DEBUG
            DrawDebug(spriteBatch, camera);
#endif
        }

#if DEBUG
        private void DrawDebug(SpriteBatch spriteBatch, Camera2d camera)
        {
            foreach (GameObject obj in _objects)
            {
                spriteBatch.DrawCircleF(obj.BoundBox.Position, obj.BoundBox.Radius, Color.Purple, .5f * camera.Zoom);
                spriteBatch.DrawLine(obj.Position, obj.Position.InDirection(obj.MovingDirection, obj.Velocity * 500), Color.Blue, 2f / camera.Zoom, 1);
                spriteBatch.DrawString(_font, $"{obj.GetType().Name}", obj.BoundBox.ToRectangleF().TopLeft, Color.Purple, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 1);
            }
        }
#endif
    }
}
