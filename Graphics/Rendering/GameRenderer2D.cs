// GameRenderer2D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

#if DEBUG
using MonoGame.Extended;
# endif
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Core.Extensions;
using MonoKit.Gameplay;
using MonoKit.Graphics.Camera;
using MonoKit.SpatialManagement;

namespace MonoKit.Graphics.Rendering
{
    public class GameRenderer2D(RuntimeContainer services, IGameObjRenderer2D renderer)
    {
        private readonly IGameObjRenderer2D _gameObjRenderer = renderer;
        private readonly RuntimeContainer _services = services;
        private readonly Camera2D _camera = services.Get<Camera2D>();
        private readonly SpatialHashing _spatialHashing = services.Get<SpatialHashing>();
        private readonly List<GameObject> _culledObjects = new();
        private float _viewportScale;

        public void Update(double elapsedMilliseconds, float viewportScale)
        {
            _culledObjects.Clear();
            _spatialHashing.GetInRectangle(_camera.Bounds, _culledObjects);
            _gameObjRenderer.UpdateEffects(elapsedMilliseconds);
            _viewportScale = viewportScale;
        }

        public void Begin(SpriteBatch spriteBatch)
        {
            _camera.UpdateView(_viewportScale);
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
        void UpdateEffects(double elapsedMilliseconds);
        void DrawEffects(SpriteBatch spriteBatch, Matrix Transformation, RuntimeContainer services, IReadOnlyList<GameObject> gameObjects);
        void DrawTextures(SpriteBatch spriteBatch, RuntimeContainer services, IReadOnlyList<GameObject> gameObjects);
    }
}
