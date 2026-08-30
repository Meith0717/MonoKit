// EntityRecords.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs;

public readonly struct EntityRecord(Archetype archetype, int row)
{
    public readonly Archetype Archetype = archetype;
    public readonly int Row = row;
}

public sealed class EntityRecords : IDisposable
{
    private readonly Dictionary<Entity, EntityRecord> _entityToRecord = new();

    public bool TryGetLocation(Entity entity, out Archetype archetype, out int row)
    {
        if (_entityToRecord.TryGetValue(entity, out var entityRecord))
        {
            archetype = entityRecord.Archetype;
            row = entityRecord.Row;
            return true;
        }
        archetype = null!;
        row = -1;
        return false;
    }

    public EntityRecord GetLocation(Entity entity)
    {
        return _entityToRecord.TryGetValue(entity, out var location)
            ? location
            : throw new ArgumentException("Entity does not exist or has been destroyed");
    }

    public void SetLocation(Entity entity, Archetype archetype, int row)
    {
        _entityToRecord[entity] = new EntityRecord(archetype, row);
    }

    public bool Remove(Entity entity)
    {
        return _entityToRecord.Remove(entity);
    }

    public bool Contains(Entity entity) => _entityToRecord.ContainsKey(entity);

    public Archetype? GetArchetype(Entity entity)
    {
        if (_entityToRecord.TryGetValue(entity, out var location))
            return location.Archetype;
        return null;
    }

    public int GetRow(Entity entity)
    {
        if (_entityToRecord.TryGetValue(entity, out var location))
            return location.Row;
        return -1;
    }

    public int Count => _entityToRecord.Count;

    public void Clear()
    {
        _entityToRecord.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
