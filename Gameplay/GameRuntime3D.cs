// GameRuntime.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

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
        _world.Update(elapsedMilliseconds);
        _camera.Update(elapsedMilliseconds, inputHandler);
    }
}
