// EntityComponentPool.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace MonoKit.Ecs.Components;

public class ComponentPool<T> : IComponentPool
    where T : struct, IComponent
{
    public int Count { get; private set; } = 0;

    public ReadOnlySpan<T> AsSpan() => _dense.AsSpan(0, Count);

    public ReadOnlySpan<int> EntitiesAsSpan() => _denseEntities.AsSpan(0, Count);

    private int[] _sparse = new int[128];
    private T[] _dense = new T[128];
    private int[] _denseEntities = new int[128];

    public ComponentPool()
    {
        Array.Fill(_sparse, -1);
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

    public void Add(int entityId, T component)
    {
        EnsureSparse(entityId);

        if (_sparse[entityId] != -1)
        {
            _dense[_sparse[entityId]] = component;
            return;
        }

        EnsureDense();

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

        // Swap with the last element to keep the array dense
        _dense[denseIndex] = _dense[lastIndex];
        var movedEntity = _denseEntities[lastIndex];
        _denseEntities[denseIndex] = movedEntity;

        // Update the sparse map for the moved entity
        _sparse[movedEntity] = denseIndex;

        // Clear the removed entity
        _sparse[entityId] = -1;
        Count--;
    }

    public bool Has(int entityId)
    {
        return entityId < _sparse.Length && _sparse[entityId] != -1;
    }

    public ref T Get(int entityId)
    {
        return ref _dense[_sparse[entityId]];
    }

    public ref T GetAt(int index) => ref _dense[index];

    public int GetEntityAt(int index) => _denseEntities[index];
}
