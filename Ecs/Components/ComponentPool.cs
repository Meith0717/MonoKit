// ComponentPool.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.Ecs.Components;

public class ComponentPool<T> : IComponentPool
    where T : struct
{
    public int Count { get; private set; } = 0;

    public ReadOnlySpan<T> AsSpan() => _dense.AsSpan(0, Count);

    public ReadOnlySpan<int> EntitiesAsSpan() => _denseEntities.AsSpan(0, Count);

    private int[] _sparse = new int[128]; // The index = entityId contains the index of the component
    private T[] _dense = new T[128]; // Contains the Components
    private int[] _denseEntities = new int[128];

    internal ComponentPool() => Array.Fill(_sparse, -1);

    public void Add(int entityId, T component)
    {
        EnsureSparse(entityId);

        // Update the component
        if (_sparse[entityId] != -1)
        {
            var componentIndex = _sparse[entityId];
            _dense[componentIndex] = component;
            return;
        }

        EnsureDense();
        // Add new component
        var denseIndex = Count++;
        _sparse[entityId] = denseIndex;
        _dense[denseIndex] = component;
        _denseEntities[denseIndex] = entityId;
    }

    public void Remove(int entityId)
    {
        if (entityId >= _sparse.Length)
            return;

        var denseIndex = _sparse[entityId];
        if (denseIndex == -1)
            return;

        var lastIndex = Count - 1;

        _dense[denseIndex] = _dense[lastIndex];
        var movedEntity = _denseEntities[lastIndex];
        _denseEntities[denseIndex] = movedEntity;

        _sparse[movedEntity] = denseIndex;

        _sparse[entityId] = -1;
        Count--;
    }

    public bool Has(int entityId)
    {
        return entityId < _sparse.Length && _sparse[entityId] != -1;
    }

    public ref T Get(int entityId)
    {
        if (!Has(entityId))
            throw new ArgumentException($"Entity {entityId} has no component {typeof(T).Name}.");

        var componentIndex = _sparse[entityId];
        ref var component = ref _dense[componentIndex];
        return ref component;
    }

    public int GetEntityAt(int index) => _denseEntities[index];

    public IEnumerable<(int entityId, int denseIndex, int hash)> GetMappings()
    {
        for (var i = 0; i < Count; i++)
            yield return (_denseEntities[i], i, _dense[i].GetHashCode());
    }

    private void EnsureSparse(int entityId)
    {
        if (entityId < _sparse.Length)
            return;

        var oldSize = _sparse.Length;
        var newSize = oldSize;
        while (newSize <= entityId)
            newSize *= 2;

        Array.Resize(ref _sparse, newSize);
        Array.Fill(_sparse, -1, oldSize, newSize - oldSize);
    }

    private void EnsureDense()
    {
        if (Count < _dense.Length)
            return;

        var newSize = _dense.Length * 2;
        Array.Resize(ref _dense, newSize);
        Array.Resize(ref _denseEntities, newSize);
    }
}
