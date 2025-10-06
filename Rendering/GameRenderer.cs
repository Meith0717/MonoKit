// GameRenderer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

#if DEBUG
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using GameEngine.Extensions;
# endif
using GameEngine.Camera;
using GameEngine.Gameplay;
using GameEngine.Runtime;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace GameEngine.Rendering
{
    public class GameRenderer(RuntimeContainer services, IGameObjRenderer renderer)
    {
        private readonly IGameObjRenderer _gameObjRenderer = renderer;
        private readonly RuntimeContainer _services = services;
        private readonly Camera2d _camera = services.Get<Camera2d>();
        private readonly SpatialHashing _spatialHashing = services.Get<SpatialHashing>();
        private readonly List<GameObject> _culledObjects = new();

        public void GetVisibleObjects()
        {
            _culledObjects.Clear();
            _spatialHashing.GetObjectsInRectangle(_camera.Bounds, _culledObjects);
        }

        public void Begin(SpriteBatch spriteBatch, float viewportScale)
        {
            _camera.UpdateTransformation(viewportScale);
            spriteBatch.Begin(transformMatrix: _camera.WorldToCamera, sortMode: SpriteSortMode.BackToFront);
        }

        public void DrawTextures(SpriteBatch spriteBatch)
        {   
            _gameObjRenderer?.DrawTextures(spriteBatch, _services, _culledObjects);

            #if DEBUG
            _spatialHashing?.Draw(spriteBatch, _camera.Position, _camera.Zoom);
            for (int i = 0; i < _culledObjects.Count; i++)
            {
                var obj = _culledObjects[i];
                spriteBatch.DrawRectangle(obj.BoundBox.ToRectangleF(), Color.Purple, 2 / _camera.Zoom);
                spriteBatch.DrawLine(obj.Position, obj.Position.InDirection(obj.MovingDirection, obj.Velocity * 500), Color.Blue, 1 / _camera.Zoom, 1);
            }
            #endif
        }

        public void DrawShaders(SpriteBatch spriteBatch)
        {
            _gameObjRenderer?.DrawShaders(spriteBatch, _services, _culledObjects);
        }
    }

    public interface IGameObjRenderer
    {
        public void DrawTextures(SpriteBatch spriteBatch, RuntimeContainer services, IReadOnlyList<GameObject> gameObjects);

        public void DrawShaders(SpriteBatch spriteBatch, RuntimeContainer services, IReadOnlyList<GameObject> gameObjects);
    }
}
