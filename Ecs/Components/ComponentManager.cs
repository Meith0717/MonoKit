// ComponentManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using MonoKit.Ecs.Entities;

namespace MonoKit.Ecs.Components;

public class ComponentManager
{
    private readonly Dictionary<Type, object> _pools = [];

    public event Action<Entity, Type> OnComponentAdded;
    public event Action<Entity, Type> OnComponentRemoved;

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

    public ref T GetComponent<T>(Entity entity)
        where T : struct
    {
        if (!TryGetPool<T>(out var pool))
            throw new KeyNotFoundException();

        ref var component = ref pool.Get(entity.Id);
        return ref component;
    }

    public void Add<T>(Entity entity, T component)
        where T : struct
    {
        GetOrCreatePool<T>().Add(entity.Id, component);
        OnComponentAdded?.Invoke(entity, typeof(T));
    }

    public void RemoveComponent<T>(Entity entity)
        where T : struct
    {
        if (TryGetPool<T>(out var pool))
        {
            pool.Remove(entity.Id);
            OnComponentRemoved?.Invoke(entity, typeof(T));
        }
    }

    internal void RemoveAllComponents(Entity entity)
    {
        foreach (var kvp in _pools)
        {
            var pool = (IComponentPool)kvp.Value;
            if (pool.Has(entity.Id))
            {
                pool.Remove(entity.Id);
                OnComponentRemoved?.Invoke(entity, kvp.Key);
            }
        }
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

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("--- Component Pools ---");
        foreach (var entry in _pools)
        {
            var pool = (IComponentPool)entry.Value;
            sb.AppendLine($"Pool<{entry.Key.Name}> (Count: {pool.Count})");

            foreach (var (entityId, denseIndex, hash) in pool.GetMappings())
                sb.AppendLine($"  Entity {entityId:D3} -> DenseIndex {denseIndex:D3} (#{hash})");
        }
        return sb.ToString();
    }
}
