// Archetype.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using MonoKit.ArchetypeEcs.Entities;
using MonoKit.ArchetypeEcs.SpatialQueries;

namespace MonoKit.ArchetypeEcs.Archetypes;

internal sealed class Archetype : IDisposable
{
    private const int InitialCapacity = 16;
    private object[] _columns;
    private Entity[] _entityIds;
    private int _capacity;
    private readonly Dictionary<Type, int> _typeToColumnIndex;

    internal ArchetypeType Type { get; }
    internal int RowCount { get; private set; }

    internal Archetype(ArchetypeType type)
    {
        var componentTypes = type.ComponentTypes;

        Type = type;
        _columns = new object[componentTypes.Length];
        _entityIds = new Entity[InitialCapacity];
        _capacity = InitialCapacity;
        _typeToColumnIndex = new Dictionary<Type, int>(componentTypes.Length);

        for (var i = 0; i < componentTypes.Length; i++)
        {
            var componentType = componentTypes[i];
            _columns[i] = Array.CreateInstance(componentType, InitialCapacity);
            _typeToColumnIndex[componentType] = i;
        }
    }

    private void EnsureCapacity(int targetRow)
    {
        if (targetRow < _capacity)
            return;

        var newCapacity = int.Max(_capacity * 2, targetRow + 1);

        for (var i = 0; i < _columns.Length; i++)
        {
            var oldArray = (Array)_columns[i];
            var elementType = oldArray.GetType().GetElementType()!;
            var newArray = Array.CreateInstance(elementType, newCapacity);
            Array.Copy(oldArray, newArray, oldArray.Length);
            _columns[i] = newArray;
        }

        Array.Resize(ref _entityIds, newCapacity);
        _capacity = newCapacity;
    }

    internal void AddRow(Entity entity, object[] components)
    {
        if (components.Length != _columns.Length)
            throw new ArgumentException(
                $"Expected {_columns.Length} components, got {components.Length}."
            );

        EnsureCapacity(RowCount);
        _entityIds[RowCount] = entity;

        for (var i = 0; i < components.Length; i++)
        {
            var component =
                components[i]
                ?? throw new ArgumentNullException(
                    nameof(components),
                    $"Component at index {i} is null."
                );
            if (!_typeToColumnIndex.TryGetValue(component.GetType(), out var columnIndex))
                throw new ArgumentException(
                    $"Component of type {component.GetType().Name} does not belong to this archetype."
                );
            ((Array)_columns[columnIndex]).SetValue(components[i], RowCount);
        }
        RowCount++;
    }

    /// <summary>
    /// Removes the row with swap-remove. Returns the entity moved into
    /// <paramref name="row"/> from the last row, or null when the removed row was the last one.
    /// </summary>
    internal Entity? RemoveRow(int row)
    {
        Entity? movedEntity = null;
        if (row < RowCount - 1)
        {
            movedEntity = _entityIds[RowCount - 1];
            _entityIds[row] = movedEntity.Value;
            foreach (var t in _columns)
            {
                var column = (Array)t;
                var lastValue = column.GetValue(RowCount - 1);
                column.SetValue(lastValue, row);
            }
        }
        RowCount--;
        return movedEntity;
    }

    internal Entity GetEntityAt(int row) => _entityIds[row];

    internal void ApplyEntityIdOrder(int[] order, int rowCount) =>
        RowOrderUtility.PermuteColumn(_entityIds, order, rowCount);

    internal bool TryGetColumn(Type componentType, out Array column)
    {
        if (_typeToColumnIndex.TryGetValue(componentType, out var columnIndex))
        {
            column = (Array)_columns[columnIndex];
            return true;
        }
        column = null!;
        return false;
    }

    internal ref T Get<T>(int row)
        where T : struct
    {
        var type = typeof(T);
        if (!_typeToColumnIndex.TryGetValue(type, out var columnIndex))
            throw new InvalidOperationException(
                $"Component {type.Name} not found in this archetype."
            );
        return ref ((T[])_columns[columnIndex])[row];
    }

    internal bool Contains(Type componentType)
    {
        return _typeToColumnIndex.ContainsKey(componentType);
    }

    internal object[] GetRowComponents(int row)
    {
        var components = new object[Type.ComponentTypes.Length];
        for (var i = 0; i < components.Length; i++)
            components[i] = ((Array)_columns[i]).GetValue(row)!;
        return components;
    }

    public void Dispose()
    {
        foreach (var t in _columns)
            Array.Clear((Array)t, 0, _capacity);
        _columns = null!;
        _entityIds = null!;
        RowCount = 0;
        _capacity = 0;
    }
}
