// World.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

// ReSharper disable MemberCanBePrivate.Global

using System;
using MonoKit.App.Input;
using MonoKit.GameRuntime.Ecs.Components;
using MonoKit.GameRuntime.Ecs.Entities;
using MonoKit.GameRuntime.Ecs.Querying;
using MonoKit.GameRuntime.Ecs.Systems;
using MonoKit.GameRuntime.Ecs.Tags;
using MonoKit.GameRuntime.Gameplay;

namespace MonoKit.GameRuntime.Ecs;

public sealed class World : IDisposable
{
    public EntityManager EntityManager { get; } = new();
    public ComponentManager Components { get; } = new();
    public EntityTypeTracker TypeTracker { get; }
    public SystemManager Systems { get; }

    public Entity WorldEntity { get; private set; }

    public int EntityCount => EntityManager.Count;

    public World()
    {
        TypeTracker = new EntityTypeTracker();
        Components.OnComponentAdded += TypeTracker.OnComponentAdded;
        Components.OnComponentRemoved += TypeTracker.OnComponentRemoved;

        WorldEntity = EntityManager.Create();
        TypeTracker.TrackEntity(WorldEntity);
        Components.Add(WorldEntity, new WorldTag());

        Systems = new SystemManager(this);
    }

    public void Update(
        double elapsedMs,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        Systems.Update(elapsedMs, runtimeServices, inputHandler);
    }

    public Entity CreateEntity()
    {
        var entity = EntityManager.Create();
        TypeTracker.TrackEntity(entity);
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        Systems.NotifyEntityDestroyed(entity);
        TypeTracker.OnEntityDestroyed(entity);
        Components.RemoveAllComponents(entity);
        EntityManager.Destroy(entity);
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("================ WORLD STATE ================");
        sb.AppendLine(Components.ToString());
        sb.AppendLine(Systems.ToString());
        sb.AppendLine("=============================================");
        return sb.ToString();
    }

    public void Dispose()
    {
        // Dispose all managers in the correct order
        Systems.Dispose();
        Components.Dispose();
        TypeTracker.Dispose();
        EntityManager.Dispose();

        // Clean up the world entity reference
        WorldEntity = default;
    }
}
