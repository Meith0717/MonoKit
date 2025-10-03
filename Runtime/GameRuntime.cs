// GameRuntime.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Camera;
using GameEngine.Content;
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
        public readonly RuntimeServiceContainer Services = new();
        public readonly CameraMover CameraMover;
        public readonly Camera2d Camera;

        private readonly SpatialHashing _spatialHashing;
        private readonly Renderer _renderer = new();
        private HashSet<GameObject> _gameObjects = new();

        public Vector2 WorldMousePosition { get; private set; }

        public IReadOnlyList<GameObject> GameObjects
            => _gameObjects.ToArray().AsReadOnly();

        public GameRuntime(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
        {
            Camera = new(graphicsDevice);
            _spatialHashing = new(spatialHashingCellSize);

            CameraMover = new(Camera);
            Services.AddService(this);
            Services.AddService(_spatialHashing);
            Services.AddService(Camera);
        }

        public void Initialize(HashSet<GameObject> gameObjects)
        {
            _gameObjects = gameObjects;
            _gameObjects.AsParallel()
                .ForAll(_spatialHashing.Add);
        }

        public void AddGameObject(GameObject gameObject)
        {
            gameObject.Initialize(Services);
            _gameObjects.Add(gameObject);
            _spatialHashing.Add(gameObject);
        }

        public void Update(GameTime gameTime, InputState inputState)
        {
            UpdateGameObjects(gameTime.ElapsedGameTime.TotalMilliseconds);
            CameraMover.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
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
            if (Debugger.IsAttached)
            {
                var font = ContentProvider.Fonts.Get("default_font");
                _spatialHashing.Draw(spriteBatch, Camera.Position, Camera.Zoom);
                var centerPosition = Vector2.Floor(Camera.Position);
                spriteBatch.DrawString(font, $"{centerPosition.X}, {centerPosition.Y}", Camera.Position + new Vector2(10, -15) / Camera.Zoom, Color.White, 0, Vector2.Zero, 0.1f / Camera.Zoom, SpriteEffects.None, 1);
                spriteBatch.DrawString(font, $"{Camera.Zoom}", Camera.Position + new Vector2(10, -35) / Camera.Zoom, Color.White, 0, Vector2.Zero, 0.1f / Camera.Zoom, SpriteEffects.None, 1);

                spriteBatch.Draw(ContentProvider.Textures.Get("crosshair"), Camera.Position, null, Color.White, 0, new Vector2(512 / 2), .05f / Camera.Zoom, SpriteEffects.None, 1);
            }
        }

        private void UpdateGameObjects(double elapsedMs)
        {
            if (_gameObjects is null) return;
            if (_gameObjects.Count == 0) return;

            var spatialHashing = Services.Get<SpatialHashing>();

            var objects = _gameObjects.ToArray();
            foreach (var obj in objects)
            {
                if (obj == null) continue;
                obj.Update(elapsedMs, Services);
                obj.Position += obj.MovingDirection * obj.Velocity * (float)elapsedMs;

                if (!obj.IsDisposed) continue;
                _gameObjects.Remove(obj);
                spatialHashing.RemoveObject(obj);
            }
        }
    }
}