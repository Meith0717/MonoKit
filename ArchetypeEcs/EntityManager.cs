// EntityManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs;

internal class EntityManager
{
    private int _nextId = 0;
    private readonly Stack<int> _freeIds = new();
    private readonly List<bool> _alive = new();

    internal int Count { get; private set; }

    internal Entity Create()
    {
        int id;
        if (_freeIds.Count > 0)
        {
            id = _freeIds.Pop();
            if (id >= _alive.Count)
                _alive.Add(true);
            else
                _alive[id] = true;
        }
        else
        {
            id = _nextId++;
            _alive.Add(true);
        }
        Count++;
        return new Entity(id);
    }

    internal bool Destroy(Entity entity)
    {
        if (entity.Id < 0 || entity.Id >= _alive.Count)
            return false;

        if (!_alive[entity.Id])
            return false;

        _alive[entity.Id] = false;
        _freeIds.Push(entity.Id);
        Count--;
        return true;
    }

    internal bool IsAlive(Entity entity)
    {
        return entity.Id >= 0 && entity.Id < _alive.Count && _alive[entity.Id];
    }

    public void Dispose()
    {
        _nextId = 0;
        _alive.Clear();
        _freeIds.Clear();
        Count = 0;
    }
}
