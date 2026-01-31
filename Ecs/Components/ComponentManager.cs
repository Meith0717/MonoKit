// ComponentManager.cs
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

    public bool TryGetPool<T>(out ComponentPool<T> pool)
        where T : struct
    {
        var type = typeof(T);
        pool = null;

        if (!_pools.TryGetValue(type, out var value))
            return false;

        pool = (ComponentPool<T>)value;
        return true;
    }

    public ComponentPool<T> GetOrCreatePool<T>()
        where T : struct
    {
        var type = typeof(T);
        if (TryGetPool<T>(out var pool))
            return pool;

        pool = new ComponentPool<T>();
        _pools[type] = pool;
        return pool;
    }

    public void AddComponent<T>(Entity entity, T component)
        where T : struct => GetOrCreatePool<T>().Add(entity.Id, component);

    public void RemoveComponent<T>(Entity entity)
        where T : struct
    {
        if (TryGetPool<T>(out var pool))
            pool.Remove(entity.Id);
    }

    public bool TryGetComponent<T>(Entity entity, out T component)
        where T : struct
    {
        component = default;
        return TryGetPool<T>(out var pool) && pool.TryGet(entity.Id, out component);
    }

    public EntityFilter CreateFilter()
    {
        return new EntityFilter(this);
    }
}
