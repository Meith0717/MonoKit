// EntityManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs.Entities;

internal class EntityManager
{
    private int _nextId = 0;
    private readonly Stack<int> _freeIds = new();
    private readonly List<int> _generations = new();
    private readonly List<bool> _alive = new();

    internal int Count { get; private set; }

    internal Entity Create()
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
            _generations.Add(0);
        }
        Count++;
        return new Entity(id, _generations[id]);
    }

    internal bool Destroy(Entity entity)
    {
        if (!IsValidHandle(entity))
            return false;

        _alive[entity.Id] = false;
        _generations[entity.Id]++;
        _freeIds.Push(entity.Id);
        Count--;
        return true;
    }

    internal bool IsAlive(Entity entity)
    {
        return IsValidHandle(entity) && _alive[entity.Id];
    }

    private bool IsValidHandle(Entity entity)
    {
        return entity.Id >= 0
            && entity.Id < _alive.Count
            && entity.Generation == _generations[entity.Id];
    }

    public void Dispose()
    {
        _nextId = 0;
        _generations.Clear();
        _alive.Clear();
        _freeIds.Clear();
        Count = 0;
    }
}
