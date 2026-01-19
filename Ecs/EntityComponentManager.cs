// EntitiesManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.ECS;

public class EntityComponentManager
{
    private readonly Dictionary<Type, object> _pools = new();

    private EntityComponentPool<T> GetPool<T>()
        where T : struct, IEntityComponent
    {
        var type = typeof(T);

        if (_pools.TryGetValue(type, out var pool))
            return (EntityComponentPool<T>)pool;

        pool = new EntityComponentPool<T>();
        _pools[type] = pool;
        return (EntityComponentPool<T>)pool;
    }

    public void AddComponent<T>(Entity entity, T component)
        where T : struct, IEntityComponent
    {
        GetPool<T>().Add(entity.Id, component);
    }

    public void RemoveComponent<T>(Entity entity)
        where T : struct, IEntityComponent
    {
        GetPool<T>().Remove(entity.Id);
    }

    public bool TryGetComponent<T>(Entity entity, out T component)
        where T : struct, IEntityComponent
    {
        var pool = GetPool<T>();
        component = default;

        if (!pool.Has(entity.Id))
            return false;

        component = pool.Get(entity.Id);
        return true;
    }

    public IEnumerable<(Entity, T)> Query<T>()
        where T : struct, IEntityComponent
    {
        var pool = GetPool<T>();

        for (var i = 0; i < pool.Count; i++)
        {
            yield return (new Entity(pool.GetEntityAt(i)), pool.GetAt(i));
        }
    }
}
