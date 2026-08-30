// ComponentRecord.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.ArchetypeEcs;

public sealed class ComponentRecord : IDisposable
{
    private readonly Dictionary<(int, int), (ArchetypeType, int)> _fastCache = new();
    private readonly Dictionary<(Type, ArchetypeType), int> _fullCache = new();

    public int GetColumnIndex(Type componentType, ArchetypeType archetypeType)
    {
        var componentHash = componentType.GetHashCode();
        var archetypeHash = archetypeType.GetHashCode();
        var fastKey = (componentHash, archetypeHash);

        if (_fastCache.TryGetValue(fastKey, out var cached))
        {
            // Verify it's actually the same (handle hash collisions)
            if (cached.Item1.Equals(archetypeType))
                return cached.Item2;
        }

        // Compute the column index by scanning the archetype's component types
        var componentTypes = archetypeType.ComponentTypes;
        for (int i = 0; i < componentTypes.Length; i++)
        {
            if (componentTypes[i] == componentType)
            {
                _fastCache[fastKey] = (archetypeType, i);
                _fullCache[(componentType, archetypeType)] = i;
                return i;
            }
        }

        var typeNames = string.Join(", ", archetypeType.ComponentTypes.Select(t => t.Name));
        throw new InvalidOperationException(
            $"Component {componentType.Name} not found in archetype with types: [{typeNames}]"
        );
    }

    /// <summary>
    /// Generic version for type-safe access.
    /// </summary>
    public int GetColumnIndex<T>(ArchetypeType archetypeType)
        where T : struct => GetColumnIndex(typeof(T), archetypeType);

    /// <summary>
    /// Tries to get the column index without throwing.
    /// </summary>
    public bool TryGetColumnIndex(
        Type componentType,
        ArchetypeType archetypeType,
        out int columnIndex
    )
    {
        var componentHash = componentType.GetHashCode();
        var archetypeHash = archetypeType.GetHashCode();
        var fastKey = (componentHash, archetypeHash);

        if (_fastCache.TryGetValue(fastKey, out var cached))
        {
            if (cached.Item1.Equals(archetypeType))
            {
                columnIndex = cached.Item2;
                return true;
            }
        }

        // Fallback to full cache for collision cases
        var lookupKey = (componentType, archetypeType);
        if (_fullCache.TryGetValue(lookupKey, out columnIndex))
            return true;

        // Compute and cache
        var componentTypes = archetypeType.ComponentTypes;
        for (int i = 0; i < componentTypes.Length; i++)
        {
            if (componentTypes[i] == componentType)
            {
                _fastCache[fastKey] = (archetypeType, i);
                _fullCache[lookupKey] = i;
                columnIndex = i;
                return true;
            }
        }

        columnIndex = -1;
        return false;
    }

    /// <summary>
    /// Clears all cached column indices.
    /// </summary>
    public void Clear()
    {
        _fastCache.Clear();
        _fullCache.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
