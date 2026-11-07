// MonoKit - GameRuntime2D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Input;
using MonoKit.GameRuntime.Camera;
using MonoKit.GameRuntime.Ecs;
using MonoKit.GameRuntime.Ecs.Systems;
using MonoKit.GameRuntime.Spatial;

namespace MonoKit.GameRuntime.Gameplay;

public class GameRuntime2D : IDisposable
{
    public readonly RuntimeContainer Services = new();
    private readonly Camera2D _camera;
    private readonly World _world;

    public GameRuntime2D(GraphicsDevice graphicsDevice, int spatialHashingCellSize)
    {
        _world = new World();
        _camera = new Camera2D(graphicsDevice);

        var ecsSpatialHash = new EcsSpatialHash2D(spatialHashingCellSize);
        Services.AddService(_world);
        Services.AddService(_camera);
        Services.AddService(ecsSpatialHash);

        _world.Systems.Add(new SpatialHashSystem2D(ecsSpatialHash));
        _world.Systems.Add(new MovementsSystem2D());
        _world.Systems.Add(new LifetimeSystem());
    }

    public Vector2 WorldMousePosition { get; private set; }

    public void Dispose()
    {
        _world.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        _world.Update(elapsedMilliseconds, Services, inputHandler);
        _camera.Update(elapsedMilliseconds, inputHandler);
        WorldMousePosition = Vector2.Transform(
            Mouse.GetState().Position.ToVector2(),
            _camera.ViewInvert
        );
    }
}
