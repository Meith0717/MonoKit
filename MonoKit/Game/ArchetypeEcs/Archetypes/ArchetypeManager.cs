// AppHost - ArchetypeManager.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;

namespace MonoKit.Game.ArchetypeEcs.Archetypes;

internal sealed class ArchetypeManager : IDisposable
{
    private readonly Dictionary<ArchetypeType, Archetype> _archetypes = new();

    private readonly Dictionary<Type, Archetype[]> _componentTypeToArchetypeSnapshots = new();
    private readonly Dictionary<Type, HashSet<Archetype>> _componentTypeToArchetypes = new();
    private bool _snapshotsDirty;

    /// <summary>Creates the shared empty archetype.</summary>
    internal ArchetypeManager()
    {
        EmptyArchetype = GetOrCreateArchetype(ArchetypeType.Empty);
    }

    internal Archetype EmptyArchetype { get; }

    internal int ArchetypeCount => _archetypes.Count;

    internal IEnumerable<Archetype> Archetypes => _archetypes.Values;

    /// <summary>Disposes all archetypes and clears the lookup maps.</summary>
    public void Dispose()
    {
        foreach (var archetype in _archetypes.Values)
            archetype.Dispose();
        _archetypes.Clear();
        _componentTypeToArchetypes.Clear();
        _componentTypeToArchetypeSnapshots.Clear();
        _snapshotsDirty = false;
    }

    /// <summary>Returns the archetype for the type, creating it when missing.</summary>
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

    /// <summary>Returns archetypes that contain the given component type.</summary>
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
}
