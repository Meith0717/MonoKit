// EntityFilter.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;

namespace MonoKit.Ecs.Querying;

public class EntityFilter(ComponentManager manager)
{
    private readonly List<IComponentPool> _required = [];
    private readonly List<IComponentPool> _excluded = [];

    public EntityFilter With<T>()
        where T : struct
    {
        _required.Add(manager.GetPool<T>());
        return this;
    }

    public EntityFilter Without<T>()
        where T : struct
    {
        _excluded.Add(manager.GetPool<T>());
        return this;
    }

    public IEnumerable<Entity> Execute()
    {
        if (_required.Count == 0)
            yield break;

        var smallestPool = _required[0];
        for (var i = 1; i < _required.Count; i++)
        {
            if (_required[i].Count < smallestPool.Count)
                smallestPool = _required[i];
        }

        for (var i = 0; i < smallestPool.Count; i++)
        {
            var entityId = smallestPool.GetEntityAt(i);

            if (Matches(entityId))
            {
                yield return new Entity(entityId);
            }
        }
    }

    private bool Matches(int entityId)
    {
        foreach (var pool in _required)
        {
            if (!pool.Has(entityId))
                return false;
        }

        foreach (var pool in _excluded)
        {
            if (pool.Has(entityId))
                return false;
        }

        return true;
    }
}
