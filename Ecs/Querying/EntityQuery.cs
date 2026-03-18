// EntityQuery.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;

namespace MonoKit.Ecs.Querying;

public class EntityQuery(ComponentManager manager)
{
    private readonly List<IComponentPool> _required = [];
    private readonly List<IComponentPool> _excluded = [];

    public EntityQuery With<T>()
        where T : struct
    {
        if (manager.TryGetPool<T>(out var pool))
            _required.Add(pool);
        return this;
    }

    public EntityQuery Without<T>()
        where T : struct
    {
        if (manager.TryGetPool<T>(out var pool))
            _excluded.Add(pool);
        return this;
    }

    public void ForEach(Action<Entity> action)
    {
        if (_required.Count == 0)
            return;

        var smallestPool = _required[0];
        for (var i = 1; i < _required.Count; i++)
            if (_required[i].Count < smallestPool.Count)
                smallestPool = _required[i];

        var count = smallestPool.Count;
        for (var i = 0; i < count; i++)
        {
            var entityId = smallestPool.GetEntityAt(i);
            if (Matches(entityId))
                action(new Entity(entityId));
        }

        _required.Clear();
        _excluded.Clear();
    }

    private bool Matches(int entityId)
    {
        // ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var pool in _required)
            if (!pool.Has(entityId))
                return false;

        foreach (var pool in _excluded)
            if (pool.Has(entityId))
                return false;

        return true;
    }
}
