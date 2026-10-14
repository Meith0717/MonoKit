// ArchetypeManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs.Archetypes;

internal sealed class ArchetypeManager : IDisposable
{
    private readonly Dictionary<ArchetypeType, Archetype> _archetypes = new();
    private readonly Dictionary<Type, HashSet<Archetype>> _componentTypeToArchetypes = new();

    private readonly Dictionary<Type, Archetype[]> _componentTypeToArchetypeSnapshots = new();
    private bool _snapshotsDirty;

    internal Archetype EmptyArchetype { get; }

    internal int ArchetypeCount => _archetypes.Count;

    internal ArchetypeManager()
    {
        EmptyArchetype = GetOrCreateArchetype(ArchetypeType.Empty);
    }

    internal Archetype GetOrCreateArchetype(ArchetypeType archetypeType)
    {
        if (_archetypes.TryGetValue(archetypeType, out var archetype))
            return archetype;

        archetype = new Archetype(archetypeType);
        _archetypes[archetypeType] = archetype;
        _snapshotsDirty = true;

        foreach (var componentType in archetypeType.ComponentTypes)
        {
            if (!_componentTypeToArchetypes.TryGetValue(componentType, out var set))
                _componentTypeToArchetypes[componentType] = set = [];
            set.Add(archetype);
        }

        return archetype;
    }

    internal Archetype[] GetArchetypesWith(Type componentType)
    {
        if (!_componentTypeToArchetypes.TryGetValue(componentType, out var set))
            return [];

        if (_snapshotsDirty)
        {
            _componentTypeToArchetypeSnapshots.Clear();
            _snapshotsDirty = false;
        }

        if (!_componentTypeToArchetypeSnapshots.TryGetValue(componentType, out var snapshot))
            _componentTypeToArchetypeSnapshots[componentType] = snapshot = [.. set];
        return snapshot;
    }

    internal IEnumerable<Archetype> Archetypes => _archetypes.Values;

    public void Dispose()
    {
        foreach (var archetype in _archetypes.Values)
            archetype.Dispose();
        _archetypes.Clear();
        _componentTypeToArchetypes.Clear();
        _componentTypeToArchetypeSnapshots.Clear();
        _snapshotsDirty = false;
    }
}
