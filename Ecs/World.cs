// EntityComponentSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

// ReSharper disable MemberCanBePrivate.Global

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Ecs.Tags;

namespace MonoKit.Ecs;

public sealed class World
{
    public EntityManager Entities { get; } = new();
    public ComponentManager Components { get; } = new();
    public SystemManager Systems { get; } = new();

    public World()
    {
        var worldEntity = Entities.Create();
        Components.AddComponent(worldEntity, new WorldTag());
    }

    public EntityFilter Query() => new EntityFilter(Components);

    public void Update(double elapsedMs)
    {
        Systems.Update(elapsedMs, Components);
    }
}
