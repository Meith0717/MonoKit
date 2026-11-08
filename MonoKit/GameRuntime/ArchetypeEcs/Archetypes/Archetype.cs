// AppHost - Archetype.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using MonoKit.GameRuntime.ArchetypeEcs.Entities;
using MonoKit.GameRuntime.ArchetypeEcs.SpatialQueries;

namespace MonoKit.GameRuntime.ArchetypeEcs.Archetypes;

internal sealed class Archetype : IDisposable
{
    private const int InitialCapacity = 16;
    private readonly Dictionary<Type, int> _typeToColumnIndex;
    private int _capacity;
    private object[] _columns;
    private Entity[] _entityIds;

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

    internal ArchetypeType Type { get; }
    internal int RowCount { get; private set; }

    public void Dispose()
    {
        foreach (var t in _columns)
            Array.Clear((Array)t, 0, _capacity);
        _columns = null!;
        _entityIds = null!;
        RowCount = 0;
        _capacity = 0;
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

    internal void AddRow<T1>(Entity entity, T1 c1)
        where T1 : struct
    {
        EnsureCapacity(RowCount);
        _entityIds[RowCount] = entity;
        ((T1[])_columns[_typeToColumnIndex[typeof(T1)]])[RowCount] = c1;
        RowCount++;
    }

    internal void AddRow<T1, T2>(Entity entity, T1 c1, T2 c2)
        where T1 : struct
        where T2 : struct
    {
        EnsureCapacity(RowCount);
        _entityIds[RowCount] = entity;
        ((T1[])_columns[_typeToColumnIndex[typeof(T1)]])[RowCount] = c1;
        ((T2[])_columns[_typeToColumnIndex[typeof(T2)]])[RowCount] = c2;
        RowCount++;
    }

    internal void AddRow<T1, T2, T3>(Entity entity, T1 c1, T2 c2, T3 c3)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        EnsureCapacity(RowCount);
        _entityIds[RowCount] = entity;
        ((T1[])_columns[_typeToColumnIndex[typeof(T1)]])[RowCount] = c1;
        ((T2[])_columns[_typeToColumnIndex[typeof(T2)]])[RowCount] = c2;
        ((T3[])_columns[_typeToColumnIndex[typeof(T3)]])[RowCount] = c3;
        RowCount++;
    }

    internal void AddRow<T1, T2, T3, T4>(Entity entity, T1 c1, T2 c2, T3 c3, T4 c4)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        EnsureCapacity(RowCount);
        _entityIds[RowCount] = entity;
        ((T1[])_columns[_typeToColumnIndex[typeof(T1)]])[RowCount] = c1;
        ((T2[])_columns[_typeToColumnIndex[typeof(T2)]])[RowCount] = c2;
        ((T3[])_columns[_typeToColumnIndex[typeof(T3)]])[RowCount] = c3;
        ((T4[])_columns[_typeToColumnIndex[typeof(T4)]])[RowCount] = c4;
        RowCount++;
    }

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

    internal Entity GetEntityAt(int row)
    {
        return _entityIds[row];
    }

    internal void ApplyEntityIdOrder(int[] order, int rowCount)
    {
        RowOrderUtility.PermuteColumn(_entityIds, order, rowCount);
    }

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
}
