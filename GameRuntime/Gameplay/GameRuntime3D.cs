// GameRuntime3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;
using MonoKit.GameRuntime.Camera;
using MonoKit.GameRuntime.Ecs;
using MonoKit.GameRuntime.Ecs.Systems;
using MonoKit.GameRuntime.Spatial;

namespace MonoKit.GameRuntime.Gameplay;

public class GameRuntime3D : IDisposable
{
    private readonly World _world;
    private readonly Camera3D _camera;
    public readonly RuntimeContainer Services = new();

    public GameRuntime3D(GraphicsDevice graphicsDevice, float spatialHashingCellSize)
    {
        _world = new World();
        _camera = new Camera3D(Vector3.Zero, graphicsDevice);

        var ecsSpatialHash = new EcsSpatialHash3D(spatialHashingCellSize);
        Services.AddService(_world);
        Services.AddService(_camera);
        Services.AddService(ecsSpatialHash);

        _world.Systems.Add(new SpatialHashSystem3D(ecsSpatialHash));
        _world.Systems.Add(new LifetimeSystem());
    }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        _world.Update(elapsedMilliseconds, Services, inputHandler);
        _camera.Update(elapsedMilliseconds, inputHandler);
    }

    public void Dispose()
    {
        _world.Dispose();
        GC.SuppressFinalize(this);
    }
}
