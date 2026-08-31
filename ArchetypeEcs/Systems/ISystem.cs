// ISystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.ArchetypeEcs.Systems;

public interface ISystem
{
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
