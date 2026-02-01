// EntityManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;

namespace MonoKit.Ecs.Entities;

public readonly struct Entity(int id)
{
    public readonly int Id = id;
}

public sealed class EntityManager
{
    private int _nextId = 0;
    private readonly Stack<int> _freeIds = new();
    private readonly List<bool> _alive = new();

    public Entity Create()
    {
        int id;
        if (_freeIds.Count > 0)
        {
            id = _freeIds.Pop();
            _alive[id] = true;
        }
        else
        {
            id = _nextId++;
            _alive.Add(true);
        }
        return new Entity(id);
    }

    public bool Destroy(Entity entity)
    {
        if (entity.Id < 0 || entity.Id >= _alive.Count)
            return false;

        if (!_alive[entity.Id])
            return false;

        _alive[entity.Id] = false;
        _freeIds.Push(entity.Id);
        return true;
    }

    public bool IsAlive(Entity entity)
    {
        return entity.Id >= 0 && entity.Id < _alive.Count && _alive[entity.Id];
    }
}
