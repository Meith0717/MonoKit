// Archetype.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs;

internal sealed class Archetype : IDisposable
{
    private const int InitialCapacity = 16;

    internal ArchetypeType Type { get; }
    internal int RowCount { get; private set; }

    private object[] _columns;
    private int[] _columnCapacities;
    private int[] _entityIds;
    private int _entityIdCapacity;
    private readonly ArchetypeManager _archetypeManager;
    private readonly Dictionary<Type, int> _typeToColumnIndex;

    internal Archetype(ArchetypeType type, ArchetypeManager archetypeManager)
    {
        Type = type;
        _archetypeManager = archetypeManager;

        var componentTypes = type.ComponentTypes;
        _columns = new object[componentTypes.Length];
        _columnCapacities = new int[componentTypes.Length];
        _typeToColumnIndex = new Dictionary<Type, int>(componentTypes.Length);

        for (var i = 0; i < componentTypes.Length; i++)
        {
            var componentType = componentTypes[i];
            _columns[i] = Array.CreateInstance(componentType, InitialCapacity);
            _columnCapacities[i] = InitialCapacity;
            _typeToColumnIndex[componentType] = i;
        }

        _entityIds = new int[InitialCapacity];
        _entityIdCapacity = InitialCapacity;
    }

    private void EnsureCapacity(int targetRow)
    {
        if (targetRow < _entityIdCapacity)
            return;

        var newCapacity = _entityIdCapacity == 0 ? InitialCapacity : _entityIdCapacity * 2;
        newCapacity = Math.Max(newCapacity, targetRow + 1);

        for (var i = 0; i < _columns.Length; i++)
        {
            if (_columnCapacities[i] >= newCapacity)
                continue;

            var oldArray = (Array)_columns[i];
            var elementType = oldArray.GetType().GetElementType()!;
            var newArray = Array.CreateInstance(elementType, newCapacity);
            Array.Copy(oldArray, newArray, oldArray.Length);
            _columns[i] = newArray;
            _columnCapacities[i] = newCapacity;
        }

        Array.Resize(ref _entityIds, newCapacity);
        _entityIdCapacity = newCapacity;
    }

    internal void AddRow(Entity entity, object[] components)
    {
        if (components.Length != _columns.Length)
            throw new ArgumentException(
                $"Expected {_columns.Length} components, got {components.Length}."
            );

        Span<int> columnIndices = stackalloc int[components.Length];
        for (var i = 0; i < components.Length; i++)
        {
            var component =
                components[i]
                ?? throw new ArgumentNullException(
                    nameof(components),
                    $"Component at index {i} is null."
                );

            if (!_typeToColumnIndex.TryGetValue(component.GetType(), out columnIndices[i]))
                throw new ArgumentException(
                    $"Component of type {component.GetType().Name} does not belong to this archetype."
                );
        }

        EnsureCapacity(RowCount);
        _entityIds[RowCount] = entity.Id;
        for (var i = 0; i < components.Length; i++)
            ((Array)_columns[columnIndices[i]]).SetValue(components[i], RowCount);
        RowCount++;
    }

    internal Entity? RemoveRow(int row)
    {
        Entity? lastEntity = null;
        if (row < RowCount - 1)
        {
            var lastEntityId = _entityIds[RowCount - 1];
            if (lastEntityId >= 0)
                lastEntity = new Entity(lastEntityId);
            _entityIds[row] = lastEntityId;
            foreach (var t in _columns)
            {
                var column = (Array)t;
                var lastValue = column.GetValue(RowCount - 1);
                column.SetValue(lastValue, row);
            }
        }
        RowCount--;
        return lastEntity;
    }

    internal Entity GetEntityAt(int row) => new(_entityIds[row]);

    internal bool TryGetColumn(Type componentType, out Array column)
    {
        if (_archetypeManager.TryGetColumnIndex(componentType, Type, out var columnIndex))
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
        var columnIndex = _archetypeManager.GetColumnIndex<T>(Type);
        return ref ((T[])_columns[columnIndex])[row];
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
        for (var i = 0; i < _columns.Length; i++)
            Array.Clear((Array)_columns[i], 0, _columnCapacities[i]);
        _columns = null!;
        _columnCapacities = null!;
        _entityIds = null!;
        RowCount = 0;
        _entityIdCapacity = 0;
    }
}
