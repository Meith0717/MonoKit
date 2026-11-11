// MonoKit - SpatialHash.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;

namespace MonoKit.Game.Spatial;

/// <summary>
///     Standalone uniform-grid spatial hash over entities, built on flat arrays: entities are
///     counted per cell in an open-addressing map, prefix-summed into cell ranges, then scattered
///     into one contiguous entry array grouped by cell. Rebuild once per frame, then query by
///     position; pair with <see cref="World.GetComponentColumn{T}" /> for component access.
///     No per-frame allocation after warmup.
/// </summary>
public sealed class SpatialHash
{
    private const long EmptySlot = -1L;

    private readonly float _inverseCellSize;
    private readonly List<Entry> _pending = new();
    private int _count;
    private Entry[] _items = [];

    private long[] _keys = [];
    private int[] _mapCounts = new int[16];
    private int[] _mapCursor = new int[16];

    private long[] _mapKeys = new long[16];
    private int _mapMask = 15;
    private int[] _mapVals = new int[16];
    private Entry[] _tmp = [];

    public SpatialHash(float cellSize)
    {
        if (cellSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        CellSize = cellSize;
        _inverseCellSize = 1f / cellSize;
    }

    public float CellSize { get; }

    /// <summary>Empties the grid and drops pending additions.</summary>
    public void Clear()
    {
        _pending.Clear();
        ResetMap();
        _count = 0;
    }

    /// <summary>Buffers one entity; applied on the next <see cref="Rebuild{TPos}" />.</summary>
    public void Add(Entity entity, Vector3 position)
    {
        _pending.Add(new Entry(entity, position));
    }

    /// <summary>Rebuilds the grid from every entity holding TPos, plus any pending additions.</summary>
    public void Rebuild<TPos>(World world, Func<TPos, Vector3> getPosition)
        where TPos : struct
    {
        EnsureCapacity(world.EntityCount + _pending.Count);
        ResetMap();

        var count = 0;
        world.ForEach(
            (Entity entity, ref TPos pos) =>
            {
                var p = getPosition(pos);
                var key = CellKey(ToCell(p.X), ToCell(p.Y), ToCell(p.Z));
                _keys[count] = key;
                _tmp[count] = new Entry(entity, p);
                _mapVals[FindSlot(key)]++;
                count++;
            }
        );

        foreach (var entry in _pending)
        {
            var p = entry.Position;
            var key = CellKey(ToCell(p.X), ToCell(p.Y), ToCell(p.Z));
            _keys[count] = key;
            _tmp[count] = entry;
            _mapVals[FindSlot(key)]++;
            count++;
        }

        _pending.Clear();

        var offset = 0;
        for (var s = 0; s <= _mapMask; s++)
        {
            if (_mapKeys[s] == EmptySlot)
                continue;
            var cellCount = _mapVals[s];
            _mapCounts[s] = cellCount;
            _mapVals[s] = offset;
            offset += cellCount;
        }

        _count = offset;
        Array.Copy(_mapVals, _mapCursor, _mapMask + 1);
        for (var i = 0; i < count; i++)
            _items[_mapCursor[FindSlot(_keys[i])]++] = _tmp[i];
    }

    /// <summary>
    ///     Collects entities within radius of center into results (cleared first, not pooled).
    ///     Returns the neighbor count; the query entity itself is included when in range.
    /// </summary>
    public int Query(Vector3 center, float radius, List<Entity> results)
    {
        results.Clear();
        if (_count == 0)
            return 0;

        var radiusSq = radius * radius;
        var startX = ToCell(center.X - radius);
        var endX = ToCell(center.X + radius) + 1;
        var startY = ToCell(center.Y - radius);
        var endY = ToCell(center.Y + radius) + 1;
        var startZ = ToCell(center.Z - radius);
        var endZ = ToCell(center.Z + radius) + 1;

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        for (var z = startZ; z < endZ; z++)
        {
            var slot = TryFindSlot(CellKey(x, y, z));
            if (slot < 0)
                continue;
            var start = _mapVals[slot];
            var cellCount = _mapCounts[slot];
            for (var i = start; i < start + cellCount; i++)
            {
                var entry = _items[i];
                var dx = entry.Position.X - center.X;
                var dy = entry.Position.Y - center.Y;
                var dz = entry.Position.Z - center.Z;
                if (dx * dx + dy * dy + dz * dz < radiusSq)
                    results.Add(entry.Entity);
            }
        }

        return results.Count;
    }

    /// <summary>Grows the entry buffers and the cell map (kept at load factor &lt;= 0.5).</summary>
    private void EnsureCapacity(int minEntries)
    {
        if (_keys.Length < minEntries)
        {
            var capacity = int.Max(minEntries, _keys.Length * 2);
            _keys = new long[capacity];
            _tmp = new Entry[capacity];
            _items = new Entry[capacity];
        }

        var mapSize = 16;
        while (mapSize < minEntries * 2)
            mapSize <<= 1;
        if (mapSize > _mapKeys.Length)
        {
            _mapKeys = new long[mapSize];
            _mapVals = new int[mapSize];
            _mapCounts = new int[mapSize];
            _mapCursor = new int[mapSize];
            _mapMask = mapSize - 1;
        }
    }

    /// <summary>Resets the cell map to empty.</summary>
    private void ResetMap()
    {
        Array.Fill(_mapKeys, EmptySlot);
        Array.Fill(_mapVals, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private int Hash(long key)
    {
        return (int)(((ulong)key * 0x9E3779B97F4A7C15UL) >> 33) & _mapMask;
    }

    /// <summary>Finds the slot for the key, inserting it when absent.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private int FindSlot(long key)
    {
        var s = Hash(key);
        while (_mapKeys[s] != EmptySlot && _mapKeys[s] != key)
            s = (s + 1) & _mapMask;
        if (_mapKeys[s] == EmptySlot)
            _mapKeys[s] = key;
        return s;
    }

    /// <summary>Finds the slot for the key, or -1 when the cell is not occupied.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private int TryFindSlot(long key)
    {
        var s = Hash(key);
        while (_mapKeys[s] != EmptySlot)
        {
            if (_mapKeys[s] == key)
                return s;
            s = (s + 1) & _mapMask;
        }

        return -1;
    }

    /// <summary>Converts a world coordinate to its cell index.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private int ToCell(float value)
    {
        return (int)float.Floor(value * _inverseCellSize);
    }

    /// <summary>Packs masked cell coordinates into a unique key.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static long CellKey(int x, int y, int z)
    {
        return ((long)(uint)(x & 0x1F_FFFF) << 42)
            | ((long)(uint)(y & 0x1F_FFFF) << 21)
            | (uint)(z & 0x1F_FFFF);
    }

    private readonly struct Entry(Entity entity, Vector3 position)
    {
        public readonly Entity Entity = entity;
        public readonly Vector3 Position = position;
    }
}
