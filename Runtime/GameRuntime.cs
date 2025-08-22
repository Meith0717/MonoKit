// GameRuntime.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Camera;
using GameEngine.Content;
using GameEngine.Gameplay;
using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameEngine.Runtime
{
    public class GameRuntime
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpatialHashing _spatialHashing;
        private HashSet<GameObject> _gameObjects;

        public readonly RuntimeServiceContainer Services;
        public readonly Camera2d Camera;
        public readonly CameraMover CameraMover;

        public Vector2 WorldMousePosition { get; private set; }

        public IReadOnlyList<GameObject> GameObjects => _gameObjects.ToArray().AsReadOnly();

        public GameRuntime(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
        {
            _graphicsDevice = graphicsDevice;
            _spatialHashing = new(spatialHashingCellSize);
            Camera = new(_graphicsDevice);
            Services = new();
            Services.AddService(this);
            Services.AddService(_spatialHashing);
            Services.AddService(Camera);
            CameraMover = new(Camera);
        }

        public void Initialize(HashSet<GameObject> gameObjects)
        {
            _gameObjects = gameObjects;
            _gameObjects.AsParallel()
                .ForAll(_spatialHashing.Add);
        }

        public void AddGameObject(GameObject gameObject)
        {
            _gameObjects.Add(gameObject);
            _spatialHashing.Add(gameObject);
        }

        public void Update(GameTime gameTime, InputState inputState)
        {
            UpdateGameObjects(gameTime.ElapsedGameTime.TotalMilliseconds);
            CameraMover.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
            _spatialHashing.Rearrange();
            WorldMousePosition = Vector2.Transform(inputState.MousePosition, Matrix.Invert(Camera.TransformationMatrix));
            if (float.IsNaN(WorldMousePosition.X) || float.IsNaN(WorldMousePosition.Y))
                WorldMousePosition = Vector2.Zero;
        }

        public void BeginDraw(SpriteBatch spriteBatch)
        {
            Camera.Update(_spatialHashing);

            spriteBatch.Begin(transformMatrix: Camera.TransformationMatrix, sortMode: SpriteSortMode.BackToFront);
        }

        public void DrawGameObjects(SpriteBatch spriteBatch)
        {
            Camera.Draw(spriteBatch, this);
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

            var objs = _gameObjects.ToArray();
            foreach (var obj in objs)
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