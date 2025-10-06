// GameRuntime.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Camera;
using GameEngine.Gameplay;
using GameEngine.Input;
using GameEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace GameEngine.Runtime
{
    public class GameRuntime
    {
        public Vector2 WorldMousePosition { get; private set; }
        public readonly RuntimeServiceContainer Services = new();

        private readonly Camera2d _camera;
        private readonly CameraMover _cameraMover;
        private readonly SpatialHashing _spatialHashing;
        private readonly GameObjManager _gameObjManager;
        private readonly Renderer _renderer = new();

        public GameRuntime(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
        {
            _camera = new(graphicsDevice);
            _cameraMover = new(_camera);
            _spatialHashing = new(spatialHashingCellSize);
            _gameObjManager = new(_spatialHashing, Services);

            Services.AddService(_camera);
            Services.AddService(_cameraMover);
            Services.AddService(_spatialHashing);
            Services.AddService(_gameObjManager);
        }

        public void Update(double elapsedMilliseconds, InputState inputState)
        {
            _gameObjManager.Update(elapsedMilliseconds);
            _spatialHashing.Rearrange();
            _cameraMover.Update(elapsedMilliseconds);
            _renderer.CullingObjects(_camera.Bounds, _spatialHashing);
            WorldMousePosition = Vector2.Transform(inputState.MousePosition, _camera.CameraToWorld);
        }

        public void BeginDraw(SpriteBatch spriteBatch, float viewportScale)
        {
            _camera.UpdateTransformation(viewportScale);
            spriteBatch.Begin(transformMatrix: _camera.WorldToCamera, sortMode: SpriteSortMode.BackToFront);
        }

        public void DrawGameObjects(SpriteBatch spriteBatch)
        {
            _renderer.Draw(spriteBatch, _camera, Services);
# if DEBUG
            var cameraPos = Vector2.Floor(_camera.Position);
            Debug.WriteLine($"Camera Pos: {cameraPos}\nCamera Zom: {_camera.Zoom}");
            _spatialHashing.Draw(spriteBatch, cameraPos, _camera.Zoom);
#endif
        }
    }
}