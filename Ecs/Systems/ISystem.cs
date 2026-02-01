// ISystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;

namespace MonoKit.Ecs.Systems;

public interface ISystem
{
    int Priority { get; }
    void Update(double elapsedMs, ComponentManager componentManager);
}

public interface IOnEntityDestroyed : ISystem
{
    void OnEntityDestroyed(Entity entity);
}
