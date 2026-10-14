// SpatialQueryContext.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoKit.ArchetypeEcs.Entities;

namespace MonoKit.ArchetypeEcs.SpatialQueries;

/// <summary>
/// Manages spatial queries and component caching for efficient neighbor iteration
/// over entities holding the position component <typeparamref name="TPos"/>.
/// All views (<see cref="SpatialField{TPos}"/>, <see cref="ComponentCache{T}"/>) are
/// valid only until the next <see cref="BeginFrame"/> call.
/// </summary>
public sealed class SpatialQueryContext<TPos> : IDisposable
    where TPos : struct
{
    private readonly SpatialEpoch _epoch = new();
    private readonly Dictionary<Type, object> _componentCaches = new();
    private readonly Dictionary<Entity, int> _entityToSpatialIndex = new();
    // Scratch buffers handed out one per ForEachNeighbor call; pooled so
    // neighbor iteration does not allocate. Nested calls rent their own buffer.
    private readonly Stack<List<int>> _scratchBuffers = new();
    private SpatialSorter<TPos>? _sorter;
    private SpatialField<TPos>? _field;

    private List<int> RentScratchBuffer() =>
        _scratchBuffers.Count > 0 ? _scratchBuffers.Pop() : new List<int>(64);

    private void ReturnScratchBuffer(List<int> buffer)
    {
        buffer.Clear();
        _scratchBuffers.Push(buffer);
    }

    /// <summary>
    /// Begins a spatial frame. Call once per frame before using neighbor queries.
    /// Returns false when no entity holds <typeparamref name="TPos"/>; in that case
    /// all views from the previous frame are invalid.
    /// </summary>
    public bool BeginFrame(World world, float cellSize, Func<TPos, Vector3> getPosition)
    {
        _epoch.Next();
        _componentCaches.Clear();
        _entityToSpatialIndex.Clear();
        _field = null;

        if (_sorter == null || _sorter.CellSize != cellSize)
            _sorter = new SpatialSorter<TPos>(cellSize, getPosition);

        if (!_sorter.TryBegin(world, _epoch.Current, out var field))
            return false;

        _field = field;
        for (var i = 0; i < field.Count; i++)
            _entityToSpatialIndex[field.GetEntity(i)] = i;

        return true;
    }

    /// <summary>Whether <paramref name="field"/> is still the current frame's field.</summary>
    public bool IsValid(SpatialField<TPos> field)
    {
        return _field is { } current && current.Version == field.Version;
    }

    /// <summary>
    /// The current frame's spatial field. Use this for hot loops that need
    /// direct index-based access with hoisted buffers (e.g. SPH pair passes);
    /// use the ForEachNeighbor overloads for simpler neighbor iteration.
    /// </summary>
    public SpatialField<TPos> Field => GetField();

    private SpatialField<TPos> GetField()
    {
        if (_field is not { } field)
            throw new InvalidOperationException(
                "No spatial frame is active. Call BeginFrame first."
            );
        return field;
    }

    /// <summary>
    /// Iterates over all entities and their neighbors with fast component access.
    /// Pairs may include the entity itself when it falls within the radius.
    /// </summary>
    public void ForEachNeighbor<T1>(float radius, RefNeighborAction<T1> action)
        where T1 : struct
    {
        var field = GetField();
        var count = field.Count;
        var cache1 = GetComponentCache<T1>();
        var neighbors = RentScratchBuffer();

        try
        {
            for (var i = 0; i < count; i++)
            {
                field.GetNeighbors(i, radius, neighbors);
                var entity = field.GetEntity(i);

                foreach (var n in neighbors)
                {
                    if (cache1.TryGet(n, out T1 c1))
                        action(entity, field.GetEntity(n), ref c1);
                }
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    /// <summary>
    /// Iterates over all entities and their neighbors with fast component access.
    /// Pairs may include the entity itself when it falls within the radius.
    /// </summary>
    public void ForEachNeighbor<T1, T2>(float radius, RefNeighborAction<T1, T2> action)
        where T1 : struct
        where T2 : struct
    {
        var field = GetField();
        var count = field.Count;
        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var neighbors = RentScratchBuffer();

        try
        {
            for (var i = 0; i < count; i++)
            {
                field.GetNeighbors(i, radius, neighbors);
                var entity = field.GetEntity(i);

                foreach (var n in neighbors)
                {
                    if (!cache1.TryGet(n, out T1 c1))
                        continue;
                    if (!cache2.TryGet(n, out T2 c2))
                        continue;
                    action(entity, field.GetEntity(n), ref c1, ref c2);
                }
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    /// <summary>Iterates over neighbors of a specific entity with fast component access.</summary>
    public void ForEachNeighbor<T1>(Entity entity, float radius, RefEntityNeighborAction<T1> action)
        where T1 : struct
    {
        var field = GetField();

        if (!_entityToSpatialIndex.TryGetValue(entity, out var index))
            return;

        var cache1 = GetComponentCache<T1>();
        var neighbors = RentScratchBuffer();

        try
        {
            field.GetNeighbors(index, radius, neighbors);

            foreach (var n in neighbors)
            {
                if (cache1.TryGet(n, out T1 c1))
                    action(field.GetEntity(n), ref c1);
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    /// <summary>Iterates over neighbors of a specific entity with fast component access.</summary>
    public void ForEachNeighbor<T1, T2>(
        Entity entity,
        float radius,
        RefEntityNeighborAction<T1, T2> action
    )
        where T1 : struct
        where T2 : struct
    {
        var field = GetField();

        if (!_entityToSpatialIndex.TryGetValue(entity, out var index))
            return;

        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var neighbors = RentScratchBuffer();

        try
        {
            field.GetNeighbors(index, radius, neighbors);

            foreach (var n in neighbors)
            {
                if (!cache1.TryGet(n, out T1 c1))
                    continue;
                if (!cache2.TryGet(n, out T2 c2))
                    continue;
                action(field.GetEntity(n), ref c1, ref c2);
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    /// <summary>
    /// Gets or creates a component cache for the current spatial frame.
    /// The cache is only valid until the next BeginFrame call.
    /// </summary>
    public ComponentCache<T> GetComponentCache<T>()
        where T : struct
    {
        if (_componentCaches.TryGetValue(typeof(T), out var cache))
            return (ComponentCache<T>)cache;

        var newCache = GetField().Cache<T>(_epoch);
        _componentCaches[typeof(T)] = newCache;
        return newCache;
    }

    public void Dispose()
    {
        _componentCaches.Clear();
        _entityToSpatialIndex.Clear();
        _scratchBuffers.Clear();
        _field = null;
        _sorter = null;
    }
}

// Neighbor iteration delegates
public delegate void RefNeighborAction<T1>(Entity entity, Entity neighbor, ref T1 nComponent1);
public delegate void RefNeighborAction<T1, T2>(
    Entity entity,
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2
);
public delegate void RefNeighborAction<T1, T2, T3>(
    Entity entity,
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2,
    ref T3 nComponent3
);
public delegate void RefNeighborAction<T1, T2, T3, T4>(
    Entity entity,
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2,
    ref T3 nComponent3,
    ref T4 nComponent4
);

// Single entity neighbor delegates
public delegate void RefEntityNeighborAction<T1>(Entity neighbor, ref T1 nComponent1);
public delegate void RefEntityNeighborAction<T1, T2>(
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2
);
