// MonoKit - EntityManager.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;

namespace MonoKit.GameRuntime.Ecs.Entities;

public readonly record struct Entity(int Id)
{
    public readonly int Id = Id;
}

public sealed class EntityManager : IDisposable
{
    private readonly List<bool> _alive = new();
    private readonly Stack<int> _freeIds = new();
    private int _nextId;

    public int Count { get; private set; }

    public void Dispose()
    {
        _nextId = 0;
        _alive.Clear();
        _freeIds.Clear();
        Count = 0;
    }

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

        Count++;
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
        Count--;
        return true;
    }

    public bool IsAlive(Entity entity)
    {
        return entity.Id >= 0 && entity.Id < _alive.Count && _alive[entity.Id];
    }
}
