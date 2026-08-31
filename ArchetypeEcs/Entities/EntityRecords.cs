// EntityRecords.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace MonoKit.ArchetypeEcs;

internal sealed class EntityRecords : IDisposable
{
    private const int InitialCapacity = 16;

    private readonly struct EntityRecord(int row, Archetype archetype)
    {
        internal readonly int Row = row;
        internal readonly Archetype Archetype = archetype;
    }

    private EntityRecord[] _entityRecords = new EntityRecord[InitialCapacity];
    private bool[] _isValid = new bool[InitialCapacity];

    private void EnsureCapacity(int entityId)
    {
        while (entityId >= _entityRecords.Length)
        {
            var newCapacity = int.Max(_entityRecords.Length * 2, entityId + 1);
            Array.Resize(ref _entityRecords, newCapacity);
            Array.Resize(ref _isValid, newCapacity);
        }
    }

    internal bool TryGetLocation(Entity entity, out Archetype archetype, out int row)
    {
        var id = entity.Id;
        if (id < _entityRecords.Length && _isValid[id])
        {
            var entityRecord = _entityRecords[id];
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
        EnsureCapacity(entity.Id);
        _entityRecords[entity.Id] = new EntityRecord(row, archetype);
        _isValid[entity.Id] = true;
    }

    internal bool Remove(Entity entity)
    {
        var id = entity.Id;
        if (id >= _entityRecords.Length)
            return false;
        _entityRecords[id] = default;
        _isValid[id] = false;
        return true;
    }

    public void Dispose()
    {
        Array.Clear(_entityRecords, 0, _entityRecords.Length);
        Array.Clear(_isValid, 0, _entityRecords.Length);
    }
}
