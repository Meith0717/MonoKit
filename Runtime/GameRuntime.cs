// GameRuntime.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

# if DEBUG
using System.Diagnostics;
#endif
using GameEngine.Camera;
using GameEngine.Gameplay;
using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Runtime
{
    public class GameRuntime
    {
        public Vector2 WorldMousePosition { get; private set; }
        public readonly RuntimeContainer Services = new();

        private readonly Camera2D _camera;
        private readonly SpatialHashing _spatialHashing;
        private readonly GameObjManager _gameObjManager;

        public GameRuntime(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
        {
            _camera = new(graphicsDevice);
            _spatialHashing = new(spatialHashingCellSize);
            _gameObjManager = new(_spatialHashing, Services);

            Services.AddService(_camera);
            Services.AddService(_spatialHashing);
            Services.AddService(_gameObjManager);
        }

        public void Update(double elapsedMilliseconds, InputState inputState)
        {
            _gameObjManager.Update(elapsedMilliseconds);
            _spatialHashing.Rearrange();
            _camera.Update(elapsedMilliseconds, inputState);
            WorldMousePosition = Vector2.Transform(inputState.MousePosition, _camera.ViewInvert);
#if DEBUG
            var cameraPos = Vector2.Floor(_camera.Position);
            Debug.WriteLine($"Camera Pos: {cameraPos}\nCamera Zom: {_camera.Zoom}");
#endif
        }
    }
}