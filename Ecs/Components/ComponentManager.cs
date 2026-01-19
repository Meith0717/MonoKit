// EntitiesManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Querying;

namespace MonoKit.Ecs.Components;

public class ComponentManager
{
    private readonly Dictionary<Type, object> _pools = new();

    public ComponentPool<T> GetPool<T>()
        where T : struct, IComponent
    {
        var type = typeof(T);

        if (_pools.TryGetValue(type, out var pool))
            return (ComponentPool<T>)pool;

        pool = new ComponentPool<T>();
        _pools[type] = pool;
        return (ComponentPool<T>)pool;
    }

    public void AddComponent<T>(Entity entity, T component)
        where T : struct, IComponent
    {
        GetPool<T>().Add(entity.Id, component);
    }

    public void RemoveComponent<T>(Entity entity)
        where T : struct, IComponent
    {
        GetPool<T>().Remove(entity.Id);
    }

    public bool TryGetComponent<T>(Entity entity, out T component)
        where T : struct, IComponent
    {
        var pool = GetPool<T>();
        component = default;

        if (!pool.Has(entity.Id))
            return false;

        component = pool.Get(entity.Id);
        return true;
    }

    public ref T GetComponentRef<T>(Entity entity)
        where T : struct, IComponent
    {
        return ref GetPool<T>().Get(entity.Id);
    }

    public EntityFilter CreateFilter()
    {
        return new EntityFilter(this);
    }
}
