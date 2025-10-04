// GameRuntime.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Camera;
using GameEngine.Gameplay;
using GameEngine.Input;
using GameEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameEngine.Runtime
{
    public class GameRuntime
    {
        public readonly Camera2d Camera;
        public readonly CameraMover CameraMover;
        public readonly RuntimeServiceContainer Services = new();

        private readonly SpatialHashing _spatialHashing;
        private readonly Renderer _renderer = new();
        private List<GameObject> _gameObjects = new();

        public Vector2 WorldMousePosition { get; private set; }

        public IReadOnlyList<GameObject> GameObjects
            => _gameObjects.ToArray().AsReadOnly();

        public GameRuntime(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
        {
            Camera = new(graphicsDevice);
            CameraMover = new(Camera);

            _spatialHashing = new(spatialHashingCellSize);

            Services.AddService(this);
            Services.AddService(_spatialHashing);
            Services.AddService(Camera);
        }

        public void Initialize(List<GameObject> gameObjects)
        {
            _gameObjects = gameObjects;
            _gameObjects.AsParallel()
                .ForAll(_spatialHashing.Add);
        }

        public void Add(GameObject gameObject)
        {
            gameObject.Initialize(Services);
            _gameObjects.Add(gameObject);
            _spatialHashing.Add(gameObject);
        }

        public void Update(double elapsedMilliseconds, InputState inputState)
        {
            UpdateGameObjects(elapsedMilliseconds);
            CameraMover.Update(elapsedMilliseconds);
            _spatialHashing.Rearrange();
            _renderer.CullingObjects(Camera.Bounds, _spatialHashing);

            WorldMousePosition = Vector2.Transform(inputState.MousePosition, Matrix.Invert(Camera.WorldToCamera));
            if (float.IsNaN(WorldMousePosition.X) || float.IsNaN(WorldMousePosition.Y))
                WorldMousePosition = Vector2.Zero;
        }

        public void BeginDraw(SpriteBatch spriteBatch, float viewportScale)
        {
            Camera.UpdateTransformation(viewportScale);
            spriteBatch.Begin(transformMatrix: Camera.WorldToCamera, sortMode: SpriteSortMode.BackToFront);
        }

        public void DrawGameObjects(SpriteBatch spriteBatch)
        {
            _renderer.Draw(spriteBatch, Camera, Services);
# if DEBUG
            var cameraPos = Vector2.Floor(Camera.Position);
            Debug.WriteLine($"Camera Pos: {cameraPos}\nCamera Zom: {Camera.Zoom}");
            _spatialHashing.Draw(spriteBatch, cameraPos, Camera.Zoom);
#endif
        }

        private void UpdateGameObjects(double elapsedMs)
        {
            if (_gameObjects is null || _gameObjects.Count == 0) return;

            for (var i = 0; i < _gameObjects.Count; i++)
            {
                var obj = _gameObjects[i];

                obj.Update(elapsedMs, Services);
                obj.Position += obj.MovingDirection * obj.Velocity * (float)elapsedMs;

                if (obj.IsDisposed)
                    _spatialHashing.RemoveObject(obj);
            }

            _gameObjects.RemoveAll(obj => obj.IsDisposed);
        }
    }
}