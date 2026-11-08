// GameRuntime2D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Input;
using MonoKit.GameRuntime.ArchetypeEcs;
using MonoKit.GameRuntime.Camera;
using MonoKit.GameRuntime.Systems;

namespace MonoKit.GameRuntime.Gameplay;

/// <summary>2D game runtime: an AECS world with lifetime + movement systems, a camera, and a service container.</summary>
public class GameRuntime2D : IDisposable
{
    public readonly RuntimeContainer Services = new();
    private readonly Camera2D _camera;
    private readonly World _world;

    public World World => _world;
    public Camera2D Camera => _camera;
    public Vector2 WorldMousePosition { get; private set; }

    public GameRuntime2D(GraphicsDevice graphicsDevice)
    {
        _world = new WorldBuilder()
            .AddSystem(new LifetimeSystem())
            .AddSystem(new MovementSystem2D())
            .Build();
        _camera = new Camera2D(graphicsDevice);

        Services.AddService(_world);
        Services.AddService(_camera);
    }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        _world.Update(elapsedMilliseconds);
        _camera.Update(elapsedMilliseconds, inputHandler);
        WorldMousePosition = Vector2.Transform(
            Mouse.GetState().Position.ToVector2(),
            _camera.ViewInvert
        );
    }

    public void Dispose()
    {
        _world.Dispose();
        GC.SuppressFinalize(this);
    }
}
