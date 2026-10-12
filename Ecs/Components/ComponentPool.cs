// ComponentPool.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonoKit.Ecs.Components;

public class ComponentPool<T> : IComponentPool, IDisposable
    where T : struct
{
    public int Count { get; private set; } = 0;

    public ReadOnlySpan<T> ComponentsAsSpan() => _denseComponents.AsSpan(0, Count);

    public ReadOnlySpan<int> EntitiesAsSpan() => _denseEntities.AsSpan(0, Count);

    private const int InitialSize = 128;
    private int[] _sparse = new int[InitialSize]; // Index = entityId - contains the index of the component
    private T[] _denseComponents = new T[InitialSize]; // Dense list containing the Components
    private int[] _denseEntities = new int[InitialSize];

    internal ComponentPool() => Array.Fill(_sparse, -1);

    public void Add(int entityId, T component)
    {
        EnsureSparse(entityId);

        if (_sparse[entityId] != -1)
        {
            var componentIndex = _sparse[entityId];
            _denseComponents[componentIndex] = component;
            return;
        }

        EnsureDense();

        var denseIndex = Count++;
        _sparse[entityId] = denseIndex;
        _denseComponents[denseIndex] = component;
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

        _denseComponents[denseIndex] = _denseComponents[lastIndex];
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

    public int GetDenseIndex(int entityId)
    {
        if (!Has(entityId))
            throw new ArgumentException($"Entity {entityId} has no component {typeof(T).Name}.");

        return _sparse[entityId];
    }

    public ref T GetByDenseComponentIndex(int denseIndex)
    {
        if (denseIndex < 0 || denseIndex >= _denseComponents.Length)
            throw new ArgumentOutOfRangeException(nameof(denseIndex));

        return ref _denseComponents[denseIndex];
    }

    public ref T Get(int entityId)
    {
        var componentIndex = GetDenseIndex(entityId);
        return ref GetByDenseComponentIndex(componentIndex);
    }

    public IEnumerable<(int entityId, int denseIndex, int hash)> GetMappings()
    {
        for (var i = 0; i < Count; i++)
            yield return (_denseEntities[i], i, _denseComponents[i].GetHashCode());
    }

    public void Dispose()
    {
        Array.Fill(_sparse, -1);
        Array.Clear(_denseComponents, 0, _denseComponents.Length);
        Array.Clear(_denseEntities, 0, _denseEntities.Length);
        Count = 0;
        GC.SuppressFinalize(this);
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
        if (Count < _denseComponents.Length)
            return;

        var newSize = _denseComponents.Length * 2;
        Array.Resize(ref _denseComponents, newSize);
        Array.Resize(ref _denseEntities, newSize);
    }
}
