// EntitiesManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using MonoKit.Gameplay;

namespace MonoKit.ECS;

public class EntitiesManager
{
    private readonly List<Entity> _entities = [];
    private readonly EntityManager _entityManager = new();
    private readonly Dictionary<Type, object> _entityComponents = [];
    private readonly Dictionary<Type, IEntityComponentSystem> _entityComponentSystems = [];

    public Entity AddEntity()
    {
        var entity = _entityManager.Create();
        _entities.Add(entity);
        return entity;
    }

    public void RemoveEntity(Entity entity)
    {
        if (_entities.Remove(entity))
            _entityManager.Destroy(entity);
    }

    public void RegisterSystem(IEntityComponentSystem system) =>
        _entityComponentSystems.TryAdd(system.GetType(), system);

    public void AddComponent(IEntityComponent system) =>
        _entityComponents.TryAdd(system.GetType(), system);

    public T GetSystem<T>()
        where T : class, IEntityComponentSystem
    {
        if (_entityComponentSystems.TryGetValue(typeof(T), out var system))
            return (T)system;

        throw new InvalidOperationException($"System {typeof(T).Name} not registered.");
    }

    public T GetComponent<T>()
        where T : class, IEntityComponent
    {
        if (_entityComponents.TryGetValue(typeof(T), out var @object))
            return (T)@object;

        throw new InvalidOperationException($"Component {typeof(T).Name} not registered.");
    }

    public void Update(double elapsedMs, RuntimeContainer runtimeContainer)
    {
        foreach (var system in _entityComponentSystems.Values)
            system.Update(elapsedMs, runtimeContainer, _entities.AsReadOnly());
    }
}
