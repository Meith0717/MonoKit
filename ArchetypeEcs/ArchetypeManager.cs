// ArchetypeManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs;

/// <summary>
/// Manages archetype creation, caching, and component-to-archetype indexing.
/// This centralizes archetype management logic for better separation of concerns.
/// </summary>
public sealed class ArchetypeManager : IDisposable
{
    private readonly ComponentRecord _componentRecord;
    
    private readonly Archetype _emptyArchetype;
    private readonly Dictionary<ArchetypeType, Archetype> _archetypes = new();
    private readonly Dictionary<Type, HashSet<Archetype>> _componentToArchetypes = new();

    public Archetype EmptyArchetype => _emptyArchetype;
    public int ArchetypeCount => _archetypes.Count;

    public ArchetypeManager(ComponentRecord componentRecord)
    {
        _componentRecord = componentRecord;
        _emptyArchetype = GetOrCreateArchetype(ArchetypeType.Empty);
    }

    /// <summary>
    /// Gets or creates an archetype for the given component types.
    /// </summary>
    public Archetype GetOrCreateArchetype(ArchetypeType type)
    {
        if (_archetypes.TryGetValue(type, out var archetype))
            return archetype;

        archetype = new Archetype(type, _componentRecord);
        _archetypes[type] = archetype;

        // Register in component-to-archetype index
        foreach (var componentType in type.ComponentTypes)
        {
            if (!_componentToArchetypes.TryGetValue(componentType, out var set))
            {
                set = new HashSet<Archetype>();
                _componentToArchetypes[componentType] = set;
            }
            set.Add(archetype);
        }

        return archetype;
    }

    /// <summary>
    /// Gets the archetype for an entity record.
    /// </summary>
    public Archetype GetArchetype(EntityRecord record) => record.Archetype;

    /// <summary>
    /// Gets all archetypes that contain the specified component type.
    /// </summary>
    public IReadOnlyCollection<Archetype>? GetArchetypesWith(Type componentType)
    {
        _componentToArchetypes.TryGetValue(componentType, out var set);
        return set;
    }

    /// <summary>
    /// Gets all archetypes that contain all of the specified component types.
    /// Returns the archetype with the fewest entities for optimization.
    /// </summary>
    public HashSet<Archetype>? GetArchetypesWith(params Type[] componentTypes)
    {
        if (componentTypes.Length == 0)
            return null;

        // Find the component type with the fewest matching archetypes
        HashSet<Archetype>? smallestSet = null;
        foreach (var type in componentTypes)
        {
            if (!_componentToArchetypes.TryGetValue(type, out var set))
                return null;
            if (smallestSet == null || set.Count < smallestSet.Count)
                smallestSet = set;
        }

        return smallestSet;
    }

    /// <summary>
    /// Checks if an archetype type exists.
    /// </summary>
    public bool HasArchetype(ArchetypeType type) => _archetypes.ContainsKey(type);

    /// <summary>
    /// Gets an existing archetype without creating it.
    /// </summary>
    public bool TryGetArchetype(ArchetypeType type, out Archetype archetype)
        => _archetypes.TryGetValue(type, out archetype);

    /// <summary>
    /// Gets the component types for all archetypes.
    /// </summary>
    public IEnumerable<ArchetypeType> ArchetypeTypes => _archetypes.Keys;

    /// <summary>
    /// Gets all archetypes.
    /// </summary>
    public IEnumerable<Archetype> Archetypes => _archetypes.Values;

    /// <summary>
    /// Creates a new archetype type from the given component types.
    /// </summary>
    public ArchetypeType CreateArchetypeType(params Type[] componentTypes)
        => new ArchetypeType(componentTypes);

    /// <summary>
    /// Creates a new archetype type by adding a component to an existing type.
    /// </summary>
    public ArchetypeType CreateArchetypeTypeWithAdded(ArchetypeType existing, Type componentType)
    {
        var allTypes = new Type[existing.ComponentTypes.Length + 1];
        existing.ComponentTypes.CopyTo(allTypes, 0);
        allTypes[existing.ComponentTypes.Length] = componentType;
        return new ArchetypeType(allTypes);
    }

    /// <summary>
    /// Creates a new archetype type by removing a component from an existing type.
    /// </summary>
    public ArchetypeType CreateArchetypeTypeWithRemoved(ArchetypeType existing, Type componentType)
    {
        var newTypes = new List<Type>(existing.ComponentTypes.Length);
        foreach (var type in existing.ComponentTypes)
        {
            if (type != componentType)
                newTypes.Add(type);
        }
        return new ArchetypeType(newTypes.ToArray());
    }

    public void Dispose()
    {
        foreach (var archetype in _archetypes.Values)
            archetype.Dispose();
        _archetypes.Clear();
        _componentToArchetypes.Clear();
    }
}
