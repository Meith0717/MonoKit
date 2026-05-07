// World.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

// ReSharper disable MemberCanBePrivate.Global

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Systems;
using MonoKit.Ecs.Tags;

namespace MonoKit.Ecs;

public sealed class World
{
    private readonly EntityManager _entityManager = new();
    public ComponentManager Components { get; } = new();
    public SystemManager Systems { get; }

    public World()
    {
        var worldEntity = _entityManager.Create();
        Components.AddComponent(worldEntity, new WorldTag());
        Systems = new SystemManager(Components);
    }

    public void Update(double elapsedMs)
    {
        Systems.Update(elapsedMs, this);
    }

    public Entity CreateEntity()
    {
        return _entityManager.Create();
    }

    public void DestroyEntity(Entity entity)
    {
        Systems.NotifyEntityDestroyed(entity);
        Components.RemoveAllComponents(entity);
        _entityManager.Destroy(entity);
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
}
