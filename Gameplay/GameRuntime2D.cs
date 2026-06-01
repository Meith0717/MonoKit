// GameRuntime2D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Ecs;
using MonoKit.Ecs.Systems;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using MonoKit.Spatial;

namespace MonoKit.Gameplay;

public class GameRuntime2D
{
    private readonly World _world;
    private readonly Camera2D _camera;
    private readonly GameObjManager _gameObjManager;
    private readonly SpatialHashing _spatialHashing;
    public readonly RuntimeContainer Services = new();

    public GameRuntime2D(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
    {
        _world = new World();
        _camera = new Camera2D(graphicsDevice);
        _spatialHashing = new SpatialHashing(spatialHashingCellSize);
        _gameObjManager = new GameObjManager(_spatialHashing, Services);

        var ecsSpatialHash = new EcsSpatialHash2D(spatialHashingCellSize);
        Services.AddService(_world);
        Services.AddService(_camera);
        Services.AddService(_spatialHashing);
        Services.AddService(ecsSpatialHash);
        Services.AddService(_gameObjManager);

        _world.Systems.Add(new SpatialHashSystem2D(ecsSpatialHash));
        _world.Systems.Add(new MovementsSystem2D());
        _world.Systems.Add(new LifetimeSystem());
    }

    public Vector2 WorldMousePosition { get; private set; }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        _world.Update(elapsedMilliseconds, Services, inputHandler);
        _gameObjManager.Update(elapsedMilliseconds);
        _spatialHashing.Rearrange();
        _camera.Update(elapsedMilliseconds, inputHandler);
        WorldMousePosition = Vector2.Transform(
            Mouse.GetState().Position.ToVector2(),
            _camera.ViewInvert
        );
    }
}
