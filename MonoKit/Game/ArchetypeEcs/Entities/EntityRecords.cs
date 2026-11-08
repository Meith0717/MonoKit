// AppHost - EntityRecords.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using MonoKit.Game.ArchetypeEcs.Archetypes;

namespace MonoKit.Game.ArchetypeEcs.Entities;

internal sealed class EntityRecords : IDisposable
{
    private const int InitialCapacity = 16;

    private EntityRecord[] _entityRecords = new EntityRecord[InitialCapacity];
    private bool[] _isValid = new bool[InitialCapacity];

    /// <summary>Clears all stored records and validity flags.</summary>
    public void Dispose()
    {
        Array.Clear(_entityRecords, 0, _entityRecords.Length);
        Array.Clear(_isValid, 0, _isValid.Length);
    }

    /// <summary>Grows the record arrays until the entity id fits.</summary>
    private void EnsureCapacity(int entityId)
    {
        while (entityId >= _entityRecords.Length)
        {
            var newCapacity = int.Max(_entityRecords.Length * 2, entityId + 1);
            Array.Resize(ref _entityRecords, newCapacity);
            Array.Resize(ref _isValid, newCapacity);
        }
    }

    /// <summary>Resolves a live entity to its archetype and row.</summary>
    internal bool TryGetLocation(Entity entity, out Archetype archetype, out int row)
    {
        var id = entity.Id;
        if (id < _entityRecords.Length && _isValid[id])
        {
            var entityRecord = _entityRecords[id];
            if (entityRecord.Generation == entity.Generation)
            {
                archetype = entityRecord.Archetype;
                row = entityRecord.Row;
                return true;
            }
        }

        archetype = null!;
        row = -1;
        return false;
    }

    /// <summary>Records where an entity lives inside its archetype.</summary>
    internal void SetEntityLocation(Entity entity, Archetype archetype, int row)
    {
        EnsureCapacity(entity.Id);
        _entityRecords[entity.Id] = new EntityRecord(row, entity.Generation, archetype);
        _isValid[entity.Id] = true;
    }

    /// <summary>Invalidates the record stored for the entity.</summary>
    internal bool Remove(Entity entity)
    {
        var id = entity.Id;
        if (id >= _entityRecords.Length)
            return false;
        _entityRecords[id] = default;
        _isValid[id] = false;
        return true;
    }

    private readonly struct EntityRecord(int row, int generation, Archetype archetype)
    {
        internal readonly int Row = row;
        internal readonly int Generation = generation;
        internal readonly Archetype Archetype = archetype;
    }
}
