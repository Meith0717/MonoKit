// MonoKit - EntityTypeTracker.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using MonoKit.GameRuntime.Ecs.Entities;

namespace MonoKit.GameRuntime.Ecs.Querying;

public class EntityTypeTracker : IDisposable
{
    private readonly Dictionary<Entity, HashSet<Type>> _entityToTypes = new();
    private readonly Dictionary<Type, HashSet<Entity>> _typeToEntities = new();

    public void Dispose()
    {
        foreach (var entities in _typeToEntities.Values)
            entities.Clear();
        _typeToEntities.Clear();

        foreach (var types in _entityToTypes.Values)
            types.Clear();
        _entityToTypes.Clear();
    }

    public void TrackEntity(Entity entity)
    {
        _entityToTypes[entity] = new HashSet<Type>();
    }

    public void OnComponentAdded(Entity entity, Type componentType)
    {
        if (!_entityToTypes.TryGetValue(entity, out var types))
            types = _entityToTypes[entity] = new HashSet<Type>();
        types.Add(componentType);

        if (!_typeToEntities.TryGetValue(componentType, out var entities))
            entities = _typeToEntities[componentType] = new HashSet<Entity>();
        entities.Add(entity);
    }

    public void OnComponentRemoved(Entity entity, Type componentType)
    {
        if (_entityToTypes.TryGetValue(entity, out var types))
            types.Remove(componentType);

        if (_typeToEntities.TryGetValue(componentType, out var entities))
            entities.Remove(entity);
    }

    public void OnEntityDestroyed(Entity entity)
    {
        if (_entityToTypes.TryGetValue(entity, out var types))
        {
            foreach (var type in types)
                if (_typeToEntities.TryGetValue(type, out var entities))
                    entities.Remove(entity);
            _entityToTypes.Remove(entity);
        }
    }

    // ==================== Single Component Queries ====================

    public IReadOnlyCollection<Entity> GetEntitiesWith<T>()
        where T : struct
    {
        return _typeToEntities.TryGetValue(typeof(T), out var set) ? set : Array.Empty<Entity>();
    }

    // ==================== Two Component Queries ====================
    public IReadOnlyCollection<Entity> GetEntitiesWith<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        if (
            !_typeToEntities.TryGetValue(typeof(T1), out var set1)
            || !_typeToEntities.TryGetValue(typeof(T2), out var set2)
        )
            return Array.Empty<Entity>();

        return set1.Count <= set2.Count ? FilterTwo(set1, set2) : FilterTwo(set2, set1);
    }

    private List<Entity> FilterTwo(HashSet<Entity> smaller, HashSet<Entity> larger)
    {
        var result = new List<Entity>(smaller.Count);
        foreach (var entity in smaller)
            if (larger.Contains(entity))
                result.Add(entity);
        return result;
    }

    // ==================== Three Component Queries ====================
    public IReadOnlyCollection<Entity> GetEntitiesWith<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        if (
            !_typeToEntities.TryGetValue(typeof(T1), out var set1)
            || !_typeToEntities.TryGetValue(typeof(T2), out var set2)
            || !_typeToEntities.TryGetValue(typeof(T3), out var set3)
        )
            return Array.Empty<Entity>();

        // Find smallest set
        var smallest = FindSmallest(set1, set2, set3);
        var other1 = GetOther(smallest, set1, set2, set3);
        var other2 = GetOther(smallest, other1, set1, set2, set3);

        return FilterThree(smallest, other1, other2);
    }

    private static HashSet<Entity> FindSmallest(
        HashSet<Entity> a,
        HashSet<Entity> b,
        HashSet<Entity> c
    )
    {
        if (a.Count <= b.Count && a.Count <= c.Count)
            return a;
        if (b.Count <= a.Count && b.Count <= c.Count)
            return b;
        return c;
    }

    private static HashSet<Entity> GetOther(
        HashSet<Entity> exclude,
        HashSet<Entity> a,
        HashSet<Entity> b,
        HashSet<Entity> c
    )
    {
        if (exclude != a)
            return a;
        if (exclude != b)
            return b;
        return c;
    }

    private static HashSet<Entity> GetOther(
        HashSet<Entity> exclude,
        HashSet<Entity> exclude2,
        HashSet<Entity> a,
        HashSet<Entity> b,
        HashSet<Entity> c
    )
    {
        if (exclude != a && exclude2 != a)
            return a;
        if (exclude != b && exclude2 != b)
            return b;
        return c;
    }

    private List<Entity> FilterThree(
        HashSet<Entity> smallest,
        HashSet<Entity> other1,
        HashSet<Entity> other2
    )
    {
        var result = new List<Entity>(smallest.Count);
        foreach (var entity in smallest)
            if (other1.Contains(entity) && other2.Contains(entity))
                result.Add(entity);
        return result;
    }

    // ==================== Four Component Queries ====================

    public IReadOnlyCollection<Entity> GetEntitiesWith<T1, T2, T3, T4>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        if (
            !_typeToEntities.TryGetValue(typeof(T1), out var set1)
            || !_typeToEntities.TryGetValue(typeof(T2), out var set2)
            || !_typeToEntities.TryGetValue(typeof(T3), out var set3)
            || !_typeToEntities.TryGetValue(typeof(T4), out var set4)
        )
            return Array.Empty<Entity>();

        // Find smallest set
        var smallest = FindSmallest(set1, set2, set3, set4);
        var others = new HashSet<Entity>[3];
        var idx = 0;
        if (set1 != smallest)
            others[idx++] = set1;
        if (set2 != smallest)
            others[idx++] = set2;
        if (set3 != smallest)
            others[idx++] = set3;
        if (set4 != smallest)
            others[idx++] = set4;

        return FilterFour(smallest, others[0], others[1], others[2]);
    }

    private static HashSet<Entity> FindSmallest(
        HashSet<Entity> a,
        HashSet<Entity> b,
        HashSet<Entity> c,
        HashSet<Entity> d
    )
    {
        var smallest = a;
        if (b.Count < smallest.Count)
            smallest = b;
        if (c.Count < smallest.Count)
            smallest = c;
        if (d.Count < smallest.Count)
            smallest = d;
        return smallest;
    }

    private List<Entity> FilterFour(
        HashSet<Entity> smallest,
        HashSet<Entity> other1,
        HashSet<Entity> other2,
        HashSet<Entity> other3
    )
    {
        var result = new List<Entity>(smallest.Count);
        foreach (var entity in smallest)
            if (other1.Contains(entity) && other2.Contains(entity) && other3.Contains(entity))
                result.Add(entity);
        return result;
    }

    // ==================== Variable Component Queries ====================

    public IReadOnlyCollection<Entity> GetEntitiesWith(params Type[] types)
    {
        if (types.Length == 0)
            return Array.Empty<Entity>();

        // Find the smallest set
        HashSet<Entity> smallest = null;
        foreach (var type in types)
        {
            if (!_typeToEntities.TryGetValue(type, out var set))
                return Array.Empty<Entity>();
            if (smallest == null || set.Count < smallest.Count)
                smallest = set;
        }

        // Collect all other sets
        var otherSets = new List<HashSet<Entity>>(types.Length - 1);
        foreach (var type in types)
            if (_typeToEntities.TryGetValue(type, out var set) && set != smallest)
                otherSets.Add(set);

        return FilterMany(smallest, otherSets);
    }

    private List<Entity> FilterMany(HashSet<Entity> smallest, List<HashSet<Entity>> others)
    {
        var result = new List<Entity>(smallest.Count);
        foreach (var entity in smallest)
        {
            var hasAll = true;
            foreach (var other in others)
                if (!other.Contains(entity))
                {
                    hasAll = false;
                    break;
                }

            if (hasAll)
                result.Add(entity);
        }

        return result;
    }

    // ==================== Utility Methods ====================

    public int GetEntityCount<T>()
        where T : struct
    {
        return _typeToEntities.TryGetValue(typeof(T), out var set) ? set.Count : 0;
    }

    public bool HasComponent(Entity entity, Type componentType)
    {
        return _entityToTypes.TryGetValue(entity, out var types) && types.Contains(componentType);
    }

    public bool HasComponent<T>(Entity entity)
        where T : struct
    {
        return HasComponent(entity, typeof(T));
    }

    // ==================== Zero-Allocation Span Methods ====================
    // These require caller to provide a buffer of sufficient size.
    // Useful when entity count is known and buffer can be pooled.

    public ReadOnlySpan<Entity> GetEntitiesWith<T>(Span<Entity> buffer)
        where T : struct
    {
        if (!_typeToEntities.TryGetValue(typeof(T), out var set) || set.Count == 0)
            return ReadOnlySpan<Entity>.Empty;

        var count = 0;
        foreach (var entity in set)
        {
            if (count >= buffer.Length)
                break;
            buffer[count++] = entity;
        }

        return buffer[..count];
    }

    public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2>(Span<Entity> buffer)
        where T1 : struct
        where T2 : struct
    {
        if (
            !_typeToEntities.TryGetValue(typeof(T1), out var set1)
            || !_typeToEntities.TryGetValue(typeof(T2), out var set2)
        )
            return ReadOnlySpan<Entity>.Empty;

        var (smaller, larger) = set1.Count <= set2.Count ? (set1, set2) : (set2, set1);

        var count = 0;
        foreach (var entity in smaller)
        {
            if (count >= buffer.Length)
                break;
            if (larger.Contains(entity))
                buffer[count++] = entity;
        }

        return buffer[..count];
    }

    public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3>(Span<Entity> buffer)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        if (
            !_typeToEntities.TryGetValue(typeof(T1), out var set1)
            || !_typeToEntities.TryGetValue(typeof(T2), out var set2)
            || !_typeToEntities.TryGetValue(typeof(T3), out var set3)
        )
            return ReadOnlySpan<Entity>.Empty;

        var smallest = FindSmallest(set1, set2, set3);
        var other1 = GetOther(smallest, set1, set2, set3);
        var other2 = GetOther(smallest, other1, set1, set2, set3);

        var count = 0;
        foreach (var entity in smallest)
        {
            if (count >= buffer.Length)
                break;
            if (other1.Contains(entity) && other2.Contains(entity))
                buffer[count++] = entity;
        }

        return buffer[..count];
    }

    public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4>(Span<Entity> buffer)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        if (
            !_typeToEntities.TryGetValue(typeof(T1), out var set1)
            || !_typeToEntities.TryGetValue(typeof(T2), out var set2)
            || !_typeToEntities.TryGetValue(typeof(T3), out var set3)
            || !_typeToEntities.TryGetValue(typeof(T4), out var set4)
        )
            return ReadOnlySpan<Entity>.Empty;

        var smallest = FindSmallest(set1, set2, set3, set4);
        var others = new HashSet<Entity>[3];
        var idx = 0;
        if (set1 != smallest)
            others[idx++] = set1;
        if (set2 != smallest)
            others[idx++] = set2;
        if (set3 != smallest)
            others[idx++] = set3;
        if (set4 != smallest)
            others[idx++] = set4;

        var count = 0;
        foreach (var entity in smallest)
        {
            if (count >= buffer.Length)
                break;
            if (
                others[0].Contains(entity)
                && others[1].Contains(entity)
                && others[2].Contains(entity)
            )
                buffer[count++] = entity;
        }

        return buffer[..count];
    }
}
