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
    private readonly Dictionary<Type, HashSet<Archetype>> _componentTypeToArchetypes = new();

    internal Archetype EmptyArchetype { get; }

    internal int ArchetypeCount => _archetypes.Count;

    internal ArchetypeManager() => EmptyArchetype = GetOrCreateArchetype(ArchetypeType.Empty);

    internal Archetype GetOrCreateArchetype(ArchetypeType archetypeType)
    {
        if (_archetypes.TryGetValue(archetypeType, out var archetype))
            return archetype;

        archetype = new Archetype(archetypeType);
        _archetypes[archetypeType] = archetype;

        foreach (var componentType in archetypeType.ComponentTypes)
        {
            if (!_componentTypeToArchetypes.TryGetValue(componentType, out var set))
                _componentTypeToArchetypes[componentType] = set = [];
            set.Add(archetype);
        }

        return archetype;
    }

    internal IReadOnlyCollection<Archetype> GetArchetypesWith(Type componentType)
    {
        _componentTypeToArchetypes.TryGetValue(componentType, out var set);
        return set;
    }

    internal IEnumerable<Archetype> Archetypes => _archetypes.Values;

    public void Dispose()
    {
        foreach (var archetype in _archetypes.Values)
            archetype.Dispose();
        _archetypes.Clear();
        _componentTypeToArchetypes.Clear();
    }
}
