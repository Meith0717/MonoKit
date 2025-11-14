// GameRuntime.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

# if DEBUG
using System.Diagnostics;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Camera;
using MonoKit.Input;

namespace MonoKit.Gameplay
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

        public void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            _gameObjManager.Update(elapsedMilliseconds);
            _spatialHashing.Rearrange();
            _camera.Update(elapsedMilliseconds, inputHandler);
            WorldMousePosition = Vector2.Transform(Mouse.GetState().Position.ToVector2(), _camera.ViewInvert);
#if DEBUG
            var cameraPos = Vector2.Floor(_camera.Position);
            System.Diagnostics.Debug.WriteLine($"Camera Pos: {cameraPos}\nCamera Zom: {_camera.Zoom}");
#endif
        }
    }
}