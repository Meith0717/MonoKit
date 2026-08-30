// Archetype.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs;

/// <summary>
/// Stores entities with identical component compositions.
/// Uses Structure of Arrays (SOA) for maximum cache efficiency.
/// Column indices are resolved via the global ComponentRecord.
/// </summary>
public sealed class Archetype : IDisposable
{
    private const int InitialCapacity = 16;

    public ArchetypeType Type { get; }
    public int RowCount { get; private set; }

    // Component data storage - Structure of Arrays (SOA)
    private object[] _columns;
    private int[] _columnCapacities;
    private int[] _entityIds;
    private int _entityIdCapacity;

    // Reference to global ComponentRecord (no local _typeToColumn anymore)
    private readonly ComponentRecord _componentRecord;

    public Archetype(ArchetypeType type, ComponentRecord componentRecord)
    {
        Type = type;
        _componentRecord = componentRecord;

        var componentTypes = type.ComponentTypes;
        _columns = new object[componentTypes.Length];
        _columnCapacities = new int[componentTypes.Length];

        for (var i = 0; i < componentTypes.Length; i++)
        {
            var componentType = componentTypes[i];
            _columns[i] = Array.CreateInstance(componentType, InitialCapacity);
            _columnCapacities[i] = InitialCapacity;
        }

        _entityIds = new int[InitialCapacity];
        _entityIdCapacity = InitialCapacity;
    }

    private void EnsureCapacity(int targetRow)
    {
        if (targetRow < RowCount)
            return;

        var newCapacity = Math.Max(_columnCapacities[0] * 2, targetRow + 1);

        for (var i = 0; i < _columns.Length; i++)
        {
            if (_columnCapacities[i] < newCapacity)
            {
                var oldArray = (Array)_columns[i];
                var elementType = oldArray.GetType().GetElementType()!;
                var newArray = Array.CreateInstance(elementType, newCapacity);
                Array.Copy(oldArray, newArray, oldArray.Length);
                _columns[i] = newArray;
                _columnCapacities[i] = newCapacity;
            }
        }

        if (targetRow >= _entityIdCapacity)
        {
            Array.Resize(ref _entityIds, _entityIdCapacity * 2);
            _entityIdCapacity = _entityIdCapacity * 2;
        }
    }

    /// <summary>
    /// Adds a new row (entity) to this archetype with the given components.
    /// </summary>
    public void AddRow(Entity entity, object[] components)
    {
        EnsureCapacity(RowCount);

        _entityIds[RowCount] = entity.Id;

        for (var i = 0; i < components.Length; i++)
        {
            ((Array)_columns[i]).SetValue(components[i], RowCount);
        }

        RowCount++;
    }

    /// <summary>
    /// Removes a row from this archetype. The last entity moves to fill the gap.
    /// Returns the entity that was moved (if any).
    /// </summary>
    public Entity? RemoveRow(int row)
    {
        Entity? movedEntity = null;

        if (row < RowCount - 1)
        {
            var lastEntityId = _entityIds[RowCount - 1];
            movedEntity = new Entity(lastEntityId);

            _entityIds[row] = lastEntityId;

            for (var i = 0; i < _columns.Length; i++)
            {
                var column = (Array)_columns[i];
                var lastValue = column.GetValue(RowCount - 1);
                column.SetValue(lastValue, row);
            }
        }

        RowCount--;
        return movedEntity;
    }

    /// <summary>
    /// Gets the entity at the specified row.
    /// </summary>
    public Entity GetEntityAt(int row) => new Entity(_entityIds[row]);

    /// <summary>
    /// Gets the component column array for a component type.
    /// Uses the global ComponentRecord for O(1) column index lookup.
    /// </summary>
    public Array GetColumn(Type componentType)
    {
        var columnIndex = _componentRecord.GetColumnIndex(componentType, Type);
        return (Array)_columns[columnIndex];
    }

    /// <summary>
    /// Tries to get the component column array for a component type.
    /// </summary>
    public bool TryGetColumn(Type componentType, out Array column)
    {
        if (_componentRecord.TryGetColumnIndex(componentType, Type, out var columnIndex))
        {
            column = (Array)_columns[columnIndex];
            return true;
        }
        column = null!;
        return false;
    }

    /// <summary>
    /// Gets a reference to a component in a specific row.
    /// Uses the global ComponentRecord for column index resolution.
    /// </summary>
    public ref T Get<T>(int row) where T : struct
    {
        var columnIndex = _componentRecord.GetColumnIndex<T>(Type);
        return ref ((T[])_columns[columnIndex])[row];
    }

    /// <summary>
    /// Gets all components for a specific row as an object array.
    /// </summary>
    public object[] GetRowComponents(int row)
    {
        var components = new object[Type.ComponentTypes.Length];
        for (var i = 0; i < components.Length; i++)
        {
            components[i] = ((Array)_columns[i]).GetValue(row)!;
        }
        return components;
    }

    /// <summary>
    /// Gets all entities in this archetype.
    /// </summary>
    public IReadOnlyList<Entity> GetEntities()
    {
        var entities = new Entity[RowCount];
        for (var i = 0; i < RowCount; i++)
        {
            entities[i] = new Entity(_entityIds[i]);
        }
        return entities;
    }

    public void Dispose()
    {
        _columns = null!;
        _columnCapacities = null!;
        _entityIds = null!;
        RowCount = 0;
        _entityIdCapacity = 0;
    }
}
