// MonoKit - ComponentColumn.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

#nullable enable

using System;
using MonoKit.Game.ArchetypeEcs.Entities;

namespace MonoKit.Game.ArchetypeEcs;

/// <summary>
///     Reusable typed accessor for component T: resolves each archetype's column array once and
///     then serves <see cref="GetRef" /> with array reads only (no per-call dictionary lookup).
///     Obtain via <see cref="World.GetComponentColumn{T}" />.
/// </summary>
public sealed class ComponentColumn<T>
    where T : struct
{
    private readonly World _world;
    private ColumnEntry[] _entries = new ColumnEntry[16];

    internal ComponentColumn(World world)
    {
        _world = world;
    }

    /// <summary>Returns whether the entity has component T.</summary>
    public bool Has(Entity entity)
    {
        return _world.HasComponent<T>(entity);
    }

    /// <summary>Returns a mutable reference to the entity's component T.</summary>
    public ref T GetRef(Entity entity)
    {
        if (!_world.TryGetLocation(entity, out var archetype, out var row))
            throw new ArgumentException("Entity does not exist or has been destroyed");

        var id = archetype.Id;
        if ((uint)id >= (uint)_entries.Length)
            Array.Resize(ref _entries, int.Max(id + 1, _entries.Length * 2));

        ref var entry = ref _entries[id];
        if (entry.Column == null || entry.Version != archetype.Version)
        {
            if (!archetype.TryGetColumn(typeof(T), out var raw))
                throw new InvalidOperationException(
                    $"Component {typeof(T).Name} is not present on this entity's archetype."
                );
            entry.Column = (T[])raw;
            entry.Version = archetype.Version;
        }

        return ref entry.Column[row];
    }

    private struct ColumnEntry
    {
        public T[]? Column;
        public int Version;
    }
}
