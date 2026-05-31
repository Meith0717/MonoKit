// ISystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.Ecs.Systems;

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
