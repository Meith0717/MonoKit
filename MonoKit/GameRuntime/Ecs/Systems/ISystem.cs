// ISystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

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
