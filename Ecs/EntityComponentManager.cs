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

    // Changed to public so advanced systems can access Pools directly
    public EntityComponentPool<T> GetPool<T>()
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

    public ref T GetComponentRef<T>(Entity entity)
        where T : struct, IEntityComponent
    {
        return ref GetPool<T>().Get(entity.Id);
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

    public IEnumerable<(Entity, T1, T2)> Query<T1, T2>()
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {
        var pool1 = GetPool<T1>();
        var pool2 = GetPool<T2>();

        if (pool1.Count < pool2.Count)
        {
            for (var i = 0; i < pool1.Count; i++)
            {
                var entityId = pool1.GetEntityAt(i);
                if (pool2.Has(entityId))
                {
                    yield return (new Entity(entityId), pool1.GetAt(i), pool2.Get(entityId));
                }
            }
        }
        else
        {
            for (var i = 0; i < pool2.Count; i++)
            {
                var entityId = pool2.GetEntityAt(i);
                if (pool1.Has(entityId))
                {
                    yield return (new Entity(entityId), pool1.Get(entityId), pool2.GetAt(i));
                }
            }
        }
    }
}
