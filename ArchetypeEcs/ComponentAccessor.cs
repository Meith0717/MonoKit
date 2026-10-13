// ComponentAccessor.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using MonoKit.ArchetypeEcs.Archtypes;
using MonoKit.ArchetypeEcs.Entities;

namespace MonoKit.ArchetypeEcs;

/// <summary>
/// Cached, allocation-free random access to all components of type T in the world.
/// Obtained via <see cref="World.CreateAccessor{T}"/>, which returns the same instance
/// for the lifetime of the world - safe to hold in a system field across frames.
/// Reads are dictionary-free array indexing. Self-heals against structural changes:
/// column reallocation is detected via per-archetype version checks, and archetypes
/// created later are picked up lazily.
/// </summary>
public sealed class ComponentAccessor<T>
    where T : struct
{
    private struct CacheSlot
    {
        public T[] Column;
        public int Version;
    }

    private readonly World _world;
    private CacheSlot[] _slots;

    internal ComponentAccessor(World world)
    {
        _world = world;
        _slots = new CacheSlot[world.ArchetypeManager.ArchetypeCount];
    }

    public ref T Get(Entity entity)
    {
        if (!_world.EntityRecords.TryGetLocation(entity, out var archetype, out var row))
            throw new ArgumentException($"Entity {entity.Id} does not exist or has been destroyed");

        var column = GetColumn(archetype);
        if (column == null)
            throw new InvalidOperationException(
                $"Entity {entity.Id} has no component {typeof(T).Name}"
            );
        return ref column[row];
    }

    public bool TryGet(Entity entity, out T value)
    {
        value = default;
        if (!_world.EntityRecords.TryGetLocation(entity, out var archetype, out var row))
            return false;

        var column = GetColumn(archetype);
        if (column == null)
            return false;
        value = column[row];
        return true;
    }

    public bool Has(Entity entity)
    {
        return _world.EntityRecords.TryGetLocation(entity, out var archetype, out _)
            && GetColumn(archetype) != null;
    }

    private T[] GetColumn(Archetype archetype)
    {
        if (archetype.Index >= _slots.Length)
            Array.Resize(ref _slots, _world.ArchetypeManager.ArchetypeCount);

        ref var slot = ref _slots[archetype.Index];
        if (slot.Version != archetype.ColumnsVersion)
        {
            slot.Column = archetype.TryGetColumn(typeof(T), out var column) ? (T[])column : null;
            slot.Version = archetype.ColumnsVersion;
        }

        return slot.Column;
    }
}
