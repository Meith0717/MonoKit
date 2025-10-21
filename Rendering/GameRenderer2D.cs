// GameRenderer2D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

#if DEBUG
using MonoGame.Extended;
using GameEngine.Extensions;
# endif
using Microsoft.Xna.Framework;
using GameEngine.Camera;
using GameEngine.Gameplay;
using GameEngine.Runtime;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameEngine.Rendering
{
    public class GameRenderer2D(RuntimeContainer services, IGameObjRenderer2D renderer)
    {
        private readonly IGameObjRenderer2D _gameObjRenderer = renderer;
        private readonly RuntimeContainer _services = services;
        private readonly Camera2D _camera = services.Get<Camera2D>();
        private readonly SpatialHashing _spatialHashing = services.Get<SpatialHashing>();
        private readonly List<GameObject> _culledObjects = new();

        public void Update(double elapsedMilliseconds)
        {
            _culledObjects.Clear();
            _spatialHashing.GetObjectsInRectangle(_camera.Bounds, _culledObjects);
            _gameObjRenderer.UpdateEffects(elapsedMilliseconds);
        }

        public void Begin(SpriteBatch spriteBatch, float viewportScale)
        {
            _camera.UpdateView(viewportScale);
            spriteBatch.Begin(transformMatrix: _camera.View, sortMode: SpriteSortMode.BackToFront);
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

        public void DrawEffects(SpriteBatch spriteBatch)
        {
            _gameObjRenderer?.DrawEffects(spriteBatch, _camera.View, _services, _culledObjects);
        }
    }

    public interface IGameObjRenderer2D
    {
        public void UpdateEffects(double elapsedMilliseconds);
        public void DrawEffects(SpriteBatch spriteBatch, Matrix Transformation, RuntimeContainer services, IReadOnlyList<GameObject> gameObjects);
        public void DrawTextures(SpriteBatch spriteBatch, RuntimeContainer services, IReadOnlyList<GameObject> gameObjects);
    }
}
