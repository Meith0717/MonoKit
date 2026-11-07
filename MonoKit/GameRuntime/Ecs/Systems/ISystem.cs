// MonoKit - ISystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.App.Input;
using MonoKit.GameRuntime.Ecs.Entities;
using MonoKit.GameRuntime.Gameplay;

namespace MonoKit.GameRuntime.Ecs.Systems;

public interface ISystem
{
    int Priority { get; }

    void Initialize(World world);

    void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    );
}

public interface IOnEntityDestroyed : ISystem
{
    void OnEntityDestroyed(Entity entity);
}
