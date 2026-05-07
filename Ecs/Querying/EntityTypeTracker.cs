// EntityTypeTracker.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using MonoKit.Ecs.Entities;

namespace MonoKit.Ecs.Querying;

public class EntityTypeTracker
{
    private readonly Dictionary<Type, HashSet<Entity>> _typeToEntities = new();
    private readonly Dictionary<Entity, HashSet<Type>> _entityToTypes = new();

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
            {
                if (_typeToEntities.TryGetValue(type, out var entities))
                    entities.Remove(entity);
            }
            _entityToTypes.Remove(entity);
        }
    }

    // Fast intersection: returns ALL entities with T1 AND T2
    public HashSet<Entity> GetEntitiesWith<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);

        if (!_typeToEntities.TryGetValue(type1, out var set1) ||
            !_typeToEntities.TryGetValue(type2, out var set2))
            return new HashSet<Entity>();

        // Intersect smaller set into larger (faster)
        if (set1.Count > set2.Count)
            (set1, set2) = (set2, set1);

        var result = new HashSet<Entity>(set1);
        result.IntersectWith(set2);
        return result;
    }

    // For 1 component
    public HashSet<Entity> GetEntitiesWith<T>() where T : struct
    {
        var type = typeof(T);
        return _typeToEntities.TryGetValue(type, out var set) 
            ? new HashSet<Entity>(set) 
            : new HashSet<Entity>();
    }

    // For 3+ components
    public HashSet<Entity> GetEntitiesWith(params Type[] types)
    {
        if (types.Length == 0) return new HashSet<Entity>();
        if (!_typeToEntities.TryGetValue(types[0], out var result))
            return new HashSet<Entity>();

        result = new HashSet<Entity>(result);
        for (int i = 1; i < types.Length; i++)
        {
            if (_typeToEntities.TryGetValue(types[i], out var next))
                result.IntersectWith(next);
            else
                return new HashSet<Entity>();
        }
        return result;
    }

    // For generic 3-4 component queries with type safety
    public HashSet<Entity> GetEntitiesWith<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);

        if (!_typeToEntities.TryGetValue(type1, out var set1) ||
            !_typeToEntities.TryGetValue(type2, out var set2) ||
            !_typeToEntities.TryGetValue(type3, out var set3))
            return new HashSet<Entity>();

        // Find smallest set
        var sets = new[] { set1, set2, set3 };
        Array.Sort(sets, (a, b) => a.Count.CompareTo(b.Count));

        var result = new HashSet<Entity>(sets[0]);
        result.IntersectWith(sets[1]);
        result.IntersectWith(sets[2]);
        return result;
    }

    public HashSet<Entity> GetEntitiesWith<T1, T2, T3, T4>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);
        var type4 = typeof(T4);

        if (!_typeToEntities.TryGetValue(type1, out var set1) ||
            !_typeToEntities.TryGetValue(type2, out var set2) ||
            !_typeToEntities.TryGetValue(type3, out var set3) ||
            !_typeToEntities.TryGetValue(type4, out var set4))
            return new HashSet<Entity>();

        var sets = new[] { set1, set2, set3, set4 };
        Array.Sort(sets, (a, b) => a.Count.CompareTo(b.Count));

        var result = new HashSet<Entity>(sets[0]);
        for (int i = 1; i < 4; i++)
            result.IntersectWith(sets[i]);
        return result;
    }

    public int GetEntityCount<T>() where T : struct
    {
        return _typeToEntities.TryGetValue(typeof(T), out var set) ? set.Count : 0;
    }

    // Zero-allocation query: caller provides buffer
    public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2>(Span<Entity> buffer)
        where T1 : struct
        where T2 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);

        if (!_typeToEntities.TryGetValue(type1, out var set1) ||
            !_typeToEntities.TryGetValue(type2, out var set2))
            return ReadOnlySpan<Entity>.Empty;

        // Intersect smaller set into buffer
        var (smaller, larger) = set1.Count < set2.Count ? (set1, set2) : (set2, set1);
        
        int count = 0;
        foreach (var e in smaller)
        {
            if (count >= buffer.Length) break;
            if (larger.Contains(e))
                buffer[count++] = e;
        }
        return buffer.Slice(0, count);
    }

    // Zero-allocation for 1 component
    public ReadOnlySpan<Entity> GetEntitiesWith<T>(Span<Entity> buffer) where T : struct
    {
        var type = typeof(T);
        if (!_typeToEntities.TryGetValue(type, out var set) || set.Count == 0)
            return ReadOnlySpan<Entity>.Empty;
        
        int count = 0;
        foreach (var e in set)
        {
            if (count >= buffer.Length) break;
            buffer[count++] = e;
        }
        return buffer.Slice(0, count);
    }

    // Zero-allocation for 3 components
    public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3>(Span<Entity> buffer)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);

        if (!_typeToEntities.TryGetValue(type1, out var set1) ||
            !_typeToEntities.TryGetValue(type2, out var set2) ||
            !_typeToEntities.TryGetValue(type3, out var set3))
            return ReadOnlySpan<Entity>.Empty;

        // Sort sets by size: smallest first
        var sets = new[] { set1, set2, set3 };
        Array.Sort(sets, (a, b) => a.Count.CompareTo(b.Count));

        int count = 0;
        foreach (var e in sets[0])
        {
            if (count >= buffer.Length) break;
            if (sets[1].Contains(e) && sets[2].Contains(e))
                buffer[count++] = e;
        }
        return buffer.Slice(0, count);
    }

    // Zero-allocation for 4 components
    public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4>(Span<Entity> buffer)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);
        var type4 = typeof(T4);

        if (!_typeToEntities.TryGetValue(type1, out var set1) ||
            !_typeToEntities.TryGetValue(type2, out var set2) ||
            !_typeToEntities.TryGetValue(type3, out var set3) ||
            !_typeToEntities.TryGetValue(type4, out var set4))
            return ReadOnlySpan<Entity>.Empty;

        var sets = new[] { set1, set2, set3, set4 };
        Array.Sort(sets, (a, b) => a.Count.CompareTo(b.Count));

        int count = 0;
        foreach (var e in sets[0])
        {
            if (count >= buffer.Length) break;
            if (sets[1].Contains(e) && sets[2].Contains(e) && sets[3].Contains(e))
                buffer[count++] = e;
        }
        return buffer.Slice(0, count);
    }

    public bool HasComponent(Entity entity, Type componentType)
    {
        return _entityToTypes.TryGetValue(entity, out var types) 
            && types.Contains(componentType);
    }

    public bool HasComponent<T>(Entity entity) where T : struct
    {
        return HasComponent(entity, typeof(T));
    }
}
