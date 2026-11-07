// MonoKit - EcsSpatialHash3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoKit.GameRuntime.Ecs.Entities;

namespace MonoKit.GameRuntime.Spatial;

public sealed class EcsSpatialHash3D(float cellSize, int capacity = 1024)
{
    public readonly float CellSize = cellSize;
    private readonly Dictionary<long, List<Entry>> _grids = new(capacity);
    private readonly float _inverseCellSize = 1f / cellSize;

    private readonly Stack<List<Entry>> _listCache = new(capacity);
    public List<Vector3> ActiveGridPositions { get; } = new(capacity);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static long MortonEncode(int x, int y, int z)
    {
        unchecked
        {
            long result = 0;
            for (var i = 0; i < 21; i++)
            {
                result |= ((long)(x >> i) & 1) << (3 * i);
                result |= ((long)(y >> i) & 1) << (3 * i + 1);
                result |= ((long)(z >> i) & 1) << (3 * i + 2);
            }

            return result;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private int ToCell(float value)
    {
        return (int)float.Floor(value * _inverseCellSize);
    }

    public void Clear()
    {
        foreach (var kvp in _grids)
        {
            kvp.Value.Clear();
            _listCache.Push(kvp.Value);
        }

        _grids.Clear();
        ActiveGridPositions.Clear();
    }

    public void Add(Entity entity, Vector3 position, Vector3 size)
    {
        var halfSize = size * 0.5f;
        var startX = ToCell(position.X - halfSize.X);
        var startY = ToCell(position.Y - halfSize.Y);
        var startZ = ToCell(position.Z - halfSize.Z);
        var endX = size == Vector3.Zero ? startX + 1 : ToCell(position.X + halfSize.X) + 1;
        var endY = size == Vector3.Zero ? startY + 1 : ToCell(position.Y + halfSize.Y) + 1;
        var endZ = size == Vector3.Zero ? startZ + 1 : ToCell(position.Z + halfSize.Z) + 1;

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        for (var z = startZ; z < endZ; z++)
        {
            var key = MortonEncode(x, y, z);
            if (!_grids.TryGetValue(key, out var cell))
            {
                var cellPos = new Vector3(x, y, z);
                _grids[key] = cell = _listCache.Count == 0 ? [] : _listCache.Pop();
                ActiveGridPositions.Add(cellPos);
            }

            cell.Add(new Entry(entity, position));
        }
    }

    public void GetInRadius(Vector3 pos, float radius, List<Entity> results)
    {
        var radiusSquared = radius * radius;

        var startX = ToCell(pos.X - radius);
        var endX = ToCell(pos.X + radius) + 1;

        var startY = ToCell(pos.Y - radius);
        var endY = ToCell(pos.Y + radius) + 1;

        var startZ = ToCell(pos.Z - radius);
        var endZ = ToCell(pos.Z + radius) + 1;

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        for (var z = startZ; z < endZ; z++)
        {
            var key = MortonEncode(x, y, z);
            if (!_grids.TryGetValue(key, out var cell))
                continue;
            for (var i = 0; i < cell.Count; i++)
            {
                var entry = cell[i];
                var dx = entry.Position.X - pos.X;
                var dy = entry.Position.Y - pos.Y;
                var dz = entry.Position.Z - pos.Z;
                if (dx * dx + dy * dy + dz * dz <= radiusSquared)
                    results.Add(entry.Entity);
            }
        }
    }

    public bool TryGetInRadius(Vector3 pos, float radius, List<Entity> results)
    {
        var radiusSquared = radius * radius;
        var hit = false;

        var startX = ToCell(pos.X - radius);
        var endX = ToCell(pos.X + radius) + 1;

        var startY = ToCell(pos.Y - radius);
        var endY = ToCell(pos.Y + radius) + 1;

        var startZ = ToCell(pos.Z - radius);
        var endZ = ToCell(pos.Z + radius) + 1;

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        for (var z = startZ; z < endZ; z++)
        {
            var key = MortonEncode(x, y, z);
            if (!_grids.TryGetValue(key, out var cell))
                continue;
            for (var i = 0; i < cell.Count; i++)
            {
                var entry = cell[i];
                var dx = entry.Position.X - pos.X;
                var dy = entry.Position.Y - pos.Y;
                var dz = entry.Position.Z - pos.Z;
                if (!(dx * dx + dy * dy + dz * dz <= radiusSquared))
                    continue;
                results.Add(entry.Entity);
                hit = true;
            }
        }

        return hit;
    }

    private struct Entry(Entity entity, Vector3 position)
    {
        public readonly Entity Entity = entity;
        public readonly Vector3 Position = position;
    }
}
