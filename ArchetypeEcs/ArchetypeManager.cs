// ArchetypeManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.ArchetypeEcs;

internal sealed class ArchetypeManager : IDisposable
{
    private readonly Dictionary<ArchetypeType, Archetype> _archetypes = new();
    private readonly Dictionary<Type, HashSet<Archetype>> _componentToArchetypes = new();
    private readonly Dictionary<(Type, ArchetypeType), int> _columnIndexCache = new();

    internal Archetype EmptyArchetype { get; }

    internal int ArchetypeCount => _archetypes.Count;

    internal ArchetypeManager()
    {
        EmptyArchetype = GetOrCreateArchetype(ArchetypeType.Empty);
    }

    internal Archetype GetOrCreateArchetype(ArchetypeType type)
    {
        if (_archetypes.TryGetValue(type, out var archetype))
            return archetype;

        archetype = new Archetype(type, this);
        _archetypes[type] = archetype;

        foreach (var componentType in type.ComponentTypes)
        {
            if (!_componentToArchetypes.TryGetValue(componentType, out var set))
            {
                set = [];
                _componentToArchetypes[componentType] = set;
            }
            set.Add(archetype);
        }

        return archetype;
    }

    internal IReadOnlyCollection<Archetype> GetArchetypesWith(Type componentType)
    {
        _componentToArchetypes.TryGetValue(componentType, out var set);
        return set;
    }

    private int GetColumnIndex(Type componentType, ArchetypeType archetypeType)
    {
        var lookupKey = (componentType, archetypeType);
        if (_columnIndexCache.TryGetValue(lookupKey, out var columnIndex))
            return columnIndex;

        var componentTypes = archetypeType.ComponentTypes;
        for (int i = 0; i < componentTypes.Length; i++)
        {
            if (componentTypes[i] == componentType)
            {
                _columnIndexCache[lookupKey] = i;
                return i;
            }
        }

        var typeNames = string.Join(", ", archetypeType.ComponentTypes.Select(t => t.Name));
        throw new InvalidOperationException(
            $"Component {componentType.Name} not found in archetype with types: [{typeNames}]"
        );
    }

    internal int GetColumnIndex<T>(ArchetypeType archetypeType)
        where T : struct => GetColumnIndex(typeof(T), archetypeType);

    internal bool TryGetColumnIndex(
        Type componentType,
        ArchetypeType archetypeType,
        out int columnIndex
    )
    {
        var lookupKey = (componentType, archetypeType);
        if (_columnIndexCache.TryGetValue(lookupKey, out columnIndex))
            return true;

        var componentTypes = archetypeType.ComponentTypes;
        for (int i = 0; i < componentTypes.Length; i++)
        {
            if (componentTypes[i] == componentType)
            {
                _columnIndexCache[lookupKey] = i;
                columnIndex = i;
                return true;
            }
        }

        columnIndex = -1;
        return false;
    }

    internal IEnumerable<Archetype> Archetypes => _archetypes.Values;

    public void Dispose()
    {
        foreach (var archetype in _archetypes.Values)
            archetype.Dispose();
        _archetypes.Clear();
        _componentToArchetypes.Clear();
        _columnIndexCache.Clear();
    }
}
