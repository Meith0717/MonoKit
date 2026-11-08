// GameRuntime3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;
using MonoKit.GameRuntime.ArchetypeEcs;
using MonoKit.GameRuntime.Camera;
using MonoKit.GameRuntime.Systems;

namespace MonoKit.GameRuntime.Gameplay;

/// <summary>3D game runtime: an AECS world with lifetime + movement systems, a 3D camera, and a service container.</summary>
public class GameRuntime3D : IDisposable
{
    public readonly RuntimeContainer Services = new();
    private readonly Camera3D _camera;
    private readonly World _world;

    public World World => _world;
    public Camera3D Camera => _camera;

    public GameRuntime3D(GraphicsDevice graphicsDevice)
    {
        _world = new WorldBuilder()
            .AddSystem(new LifetimeSystem())
            .AddSystem(new MovementSystem3D())
            .Build();
        _camera = new Camera3D(Vector3.Zero, graphicsDevice);

        Services.AddService(_world);
        Services.AddService(_camera);
    }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        _world.Update(elapsedMilliseconds);
        _camera.Update(elapsedMilliseconds, inputHandler);
    }

    public void Dispose()
    {
        _world.Dispose();
        GC.SuppressFinalize(this);
    }
}
