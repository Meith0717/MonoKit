// SpatialQueryContext.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoKit.ArchetypeEcs.Archtypes;
using MonoKit.ArchetypeEcs.Entities;

namespace MonoKit.ArchetypeEcs.SpatialQueries;

/// <summary>
/// Manages spatial queries and component caching for efficient neighbor iteration.
/// </summary>
public sealed class SpatialQueryContext : IDisposable
{
    private object? _spatialSorter;
    private object? _currentSpatialField;
    private readonly Dictionary<Type, object> _componentCaches = new();
    private readonly Dictionary<Entity, int> _entityToSpatialIndex = new();

    /// <summary>
    /// Begins a spatial frame. Call once per frame before using neighbor queries.
    /// </summary>
    public bool BeginFrame<TPos>(World world, float cellSize, Func<TPos, Vector3> getPosition)
        where TPos : struct
    {
        var sorterType = typeof(SpatialSorter<TPos>);
        
        if (_spatialSorter == null || _spatialSorter.GetType() != sorterType)
            _spatialSorter = Activator.CreateInstance(sorterType, cellSize, getPosition);

        var sorter = (dynamic)_spatialSorter;
        var success = sorter.TryBegin(world, out dynamic field);
        
        if (success)
        {
            _currentSpatialField = field;
            _componentCaches.Clear();
            _entityToSpatialIndex.Clear();
            
            // Build entity -> index mapping
            var count = (int)field.Count;
            for (int i = 0; i < count; i++)
            {
                var entity = field.GetEntity(i);
                _entityToSpatialIndex[entity] = i;
            }
        }
        
        return success;
    }

    /// <summary>
    /// Iterates over all entities and their neighbors with fast component access.
    /// </summary>
    public void ForEachNeighbor<T1, T2>(
        float radius,
        RefNeighborAction<T1, T2> action
    ) where T1 : struct where T2 : struct
    {
        if (_currentSpatialField == null)
            throw new InvalidOperationException("Call BeginFrame first");
        
        var field = (dynamic)_currentSpatialField;
        var count = (int)field.Count;
        var cache1 = GetOrCreateComponentCache<T1>();
        var cache2 = GetOrCreateComponentCache<T2>();
        var neighbors = new List<int>();

        for (int i = 0; i < count; i++)
        {
            field.GetNeighbors(i, radius, neighbors);
            var entity = field.GetEntity(i);
            
            foreach (var n in neighbors)
            {
                if (cache1.TryGet(n, out T1 c1) && cache2.TryGet(n, out T2 c2))
                    action(entity, field.GetEntity(n), ref c1, ref c2);
            }
        }
    }

    /// <summary>
    /// Iterates over all entities and their neighbors with fast component access.
    /// </summary>
    public void ForEachNeighbor<T1>(
        float radius,
        RefNeighborAction<T1> action
    ) where T1 : struct
    {
        if (_currentSpatialField == null)
            throw new InvalidOperationException("Call BeginFrame first");
        
        var field = (dynamic)_currentSpatialField;
        var count = (int)field.Count;
        var cache1 = GetOrCreateComponentCache<T1>();
        var neighbors = new List<int>();

        for (int i = 0; i < count; i++)
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

    /// <summary>
    /// Iterates over neighbors of a specific entity with fast component access.
    /// </summary>
    public void ForEachNeighbor<T1>(
        Entity entity,
        float radius,
        RefEntityNeighborAction<T1> action
    ) where T1 : struct
    {
        if (_currentSpatialField == null)
            throw new InvalidOperationException("Call BeginFrame first");
        
        if (!_entityToSpatialIndex.TryGetValue(entity, out var index))
            return;
        
        var field = (dynamic)_currentSpatialField;
        var cache1 = GetOrCreateComponentCache<T1>();
        var neighbors = new List<int>();

        field.GetNeighbors(index, radius, neighbors);
        
        foreach (var n in neighbors)
        {
            if (cache1.TryGet(n, out T1 c1))
                action(field.GetEntity(n), ref c1);
        }
    }

    /// <summary>
    /// Iterates over neighbors of a specific entity with fast component access.
    /// </summary>
    public void ForEachNeighbor<T1, T2>(
        Entity entity,
        float radius,
        RefEntityNeighborAction<T1, T2> action
    ) where T1 : struct where T2 : struct
    {
        if (_currentSpatialField == null)
            throw new InvalidOperationException("Call BeginFrame first");
        
        if (!_entityToSpatialIndex.TryGetValue(entity, out var index))
            return;
        
        var field = (dynamic)_currentSpatialField;
        var cache1 = GetOrCreateComponentCache<T1>();
        var cache2 = GetOrCreateComponentCache<T2>();
        var neighbors = new List<int>();

        field.GetNeighbors(index, radius, neighbors);
        
        foreach (var n in neighbors)
        {
            if (cache1.TryGet(n, out T1 c1) && cache2.TryGet(n, out T2 c2))
                action(field.GetEntity(n), ref c1, ref c2);
        }
    }

    /// <summary>
    /// Gets or creates a component cache for the current spatial frame.
    /// </summary>
    private ComponentCache<T> GetOrCreateComponentCache<T>() where T : struct
    {
        if (_componentCaches.TryGetValue(typeof(T), out var cache))
            return (ComponentCache<T>)cache;
        
        var field = (dynamic)_currentSpatialField;
        var newCache = field.Cache<T>();
        _componentCaches[typeof(T)] = newCache;
        return newCache;
    }

    public void Dispose()
    {
        _componentCaches.Clear();
        _entityToSpatialIndex.Clear();
        _currentSpatialField = null;
        if (_spatialSorter is IDisposable disposable)
            disposable.Dispose();
        _spatialSorter = null;
    }
}

// Neighbor iteration delegates
public delegate void RefNeighborAction<T1>(Entity entity, Entity neighbor, ref T1 nComponent1);
public delegate void RefNeighborAction<T1, T2>(Entity entity, Entity neighbor, ref T1 nComponent1, ref T2 nComponent2);
public delegate void RefNeighborAction<T1, T2, T3>(Entity entity, Entity neighbor, ref T1 nComponent1, ref T2 nComponent2, ref T3 nComponent3);
public delegate void RefNeighborAction<T1, T2, T3, T4>(Entity entity, Entity neighbor, ref T1 nComponent1, ref T2 nComponent2, ref T3 nComponent3, ref T4 nComponent4);

// Single entity neighbor delegates
public delegate void RefEntityNeighborAction<T1>(Entity neighbor, ref T1 nComponent1);
public delegate void RefEntityNeighborAction<T1, T2>(Entity neighbor, ref T1 nComponent1, ref T2 nComponent2);
