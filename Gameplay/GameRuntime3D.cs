// GameRuntime.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Ecs.Systems;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using MonoKit.Spatial;

namespace MonoKit.Gameplay;

public class GameRuntime3D
{
    private readonly World _world;
    private readonly Camera3D _camera;
    public readonly RuntimeContainer Services = new();

    [Obsolete("Use ECS systems instead")]
    private readonly GameObjManager _gameObjManager;

    [Obsolete("Use ECS systems instead")]
    private readonly SpatialHashing _spatialHashing;

    public GameRuntime3D(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
    {
        _world = new World();
        _camera = new Camera3D(Vector3.Zero, graphicsDevice);

        var ecsSpatialHash = new EcsSpatialHash3D(spatialHashingCellSize);
        Services.AddService(_world);
        Services.AddService(_camera);
        Services.AddService(ecsSpatialHash);

        _world.Systems.Add(new SpatialHashSystem3D(ecsSpatialHash));
        _world.Systems.Add(new LifetimeSystem());

        _spatialHashing = new SpatialHashing(spatialHashingCellSize);
        _gameObjManager = new GameObjManager(_spatialHashing, Services);
        Services.AddService(_spatialHashing);
        Services.AddService(_gameObjManager);
    }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        _world.Update(elapsedMilliseconds);
        _gameObjManager.Update(elapsedMilliseconds);
        _spatialHashing.Rearrange();
        _camera.Update(elapsedMilliseconds, inputHandler);
    }
}
