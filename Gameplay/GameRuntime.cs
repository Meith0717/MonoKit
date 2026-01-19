// GameRuntime.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

# if DEBUG
#endif
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Ecs;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using MonoKit.Spatial;

namespace MonoKit.Gameplay;

public class GameRuntime
{
    private readonly World _world;
    private readonly Camera2D _camera;
    private readonly GameObjManager _gameObjManager;
    private readonly SpatialHashing _spatialHashing;
    public readonly RuntimeContainer Services = new();

    public GameRuntime(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
    {
        _world = new World();
        _camera = new Camera2D(graphicsDevice);
        _spatialHashing = new SpatialHashing(spatialHashingCellSize);
        _gameObjManager = new GameObjManager(_spatialHashing, Services);

        Services.AddService(_world);
        Services.AddService(_camera);
        Services.AddService(_spatialHashing);
        Services.AddService(_gameObjManager);
    }

    public Vector2 WorldMousePosition { get; private set; }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        _world.Update(elapsedMilliseconds);
        _gameObjManager.Update(elapsedMilliseconds);
        _spatialHashing.Rearrange();
        _camera.Update(elapsedMilliseconds, inputHandler);
        WorldMousePosition = Vector2.Transform(
            Mouse.GetState().Position.ToVector2(),
            _camera.ViewInvert
        );
#if DEBUG
        var cameraPos = Vector2.Floor(_camera.Position);
        Debug.WriteLine($"Camera Pos: {cameraPos}\nCamera Zom: {_camera.Zoom}");
#endif
    }
}
