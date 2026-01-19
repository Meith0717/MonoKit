// EntityManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;

namespace MonoKit.ECS;

public readonly struct Entity(int id)
{
    public readonly int Id = id;
}

public sealed class EntityManager
{
    private int _nextId = 0;
    private readonly Stack<int> _freeIds = new();

    public Entity Create()
    {
        return _freeIds.Count > 0 ? new Entity(_freeIds.Pop()) : new Entity(_nextId++);
    }

    public void Destroy(Entity entity)
    {
        _freeIds.Push(entity.Id);
    }
}
