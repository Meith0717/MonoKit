// EntityRecords.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs;

internal readonly struct EntityRecord(Archetype archetype, int row)
{
    internal readonly Archetype Archetype = archetype;
    internal readonly int Row = row;
}

internal sealed class EntityRecords : IDisposable
{
    private readonly Dictionary<Entity, EntityRecord> _entityToRecord = new();

    internal bool TryGetLocation(Entity entity, out Archetype archetype, out int row)
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

    internal void SetEntityLocation(Entity entity, Archetype archetype, int row)
    {
        _entityToRecord[entity] = new EntityRecord(archetype, row);
    }

    internal bool Remove(Entity entity)
    {
        return _entityToRecord.Remove(entity);
    }

    public void Dispose()
    {
        _entityToRecord.Clear();
    }
}
