// EntityComponentSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

// ReSharper disable MemberCanBePrivate.Global
namespace MonoKit.ECS;

public sealed class EntityComponentSystem
{
    public EntityManager EntityManager { get; } = new();
    public EntitySystemManager EntitySystemManager { get; } = new();
    public EntityComponentManager EntityComponentManager { get; } = new();

    public EntityComponentSystem()
    {
        var world = EntityManager.Create();
        EntityComponentManager.AddComponent(world, new WorldTag());
    }

    public void Update(double elapsedMs)
    {
        EntitySystemManager.Update(elapsedMs, EntityComponentManager);
    }
}
