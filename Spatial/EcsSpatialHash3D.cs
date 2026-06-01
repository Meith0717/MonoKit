// EcsSpatialHash3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace MonoKit.Spatial;

public sealed class EcsSpatialHash3D(float cellSize, int capacity = 1024) : ISpatialGrid3D
{
    private readonly Dictionary<long, List<Entry>> _grids = new(capacity);
    private readonly List<List<Entry>> _activeCells = new(capacity);
    private readonly float _inverseCellSize = 1f / cellSize;

    private struct Entry(Entity entity, Vector3 position)
    {
        public readonly Entity Entity = entity;
        public readonly Vector3 Position = position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long Hash(int x, int y, int z)
    {
        unchecked
        {
            return ((long)x * 73856093L) ^ ((long)y * 19349663L) ^ ((long)z * 83492791L);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ToCell(float value)
    {
        return (int)float.Floor(value * _inverseCellSize);
    }

    public void Clear()
    {
        for (int i = 0; i < _activeCells.Count; i++)
        {
            _activeCells[i].Clear();
        }

        _activeCells.Clear();
    }

    public void Add(Entity entity, Vector3 position, Vector3 size)
    {
        var startX = ToCell(position.X);
        var startY = ToCell(position.Y);
        var startZ = ToCell(position.Z);

        int endX,
            endY,
            endZ;

        if (size == Vector3.Zero)
        {
            endX = startX + 1;
            endY = startY + 1;
            endZ = startZ + 1;
        }
        else
        {
            endX = (int)float.Ceiling((position.X + size.X) * _inverseCellSize);
            endY = (int)float.Ceiling((position.Y + size.Y) * _inverseCellSize);
            endZ = (int)float.Ceiling((position.Z + size.Z) * _inverseCellSize);
        }

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        for (var z = startZ; z < endZ; z++)
        {
            var hash = Hash(x, y, z);

            if (!_grids.TryGetValue(hash, out var cell))
            {
                cell = new List<Entry>();
                _grids.Add(hash, cell);
            }

            if (cell.Count == 0)
                _activeCells.Add(cell);

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
            var hash = Hash(x, y, z);

            if (!_grids.TryGetValue(hash, out var cell))
                continue;

            for (var i = 0; i < cell.Count; i++)
            {
                var entry = cell[i];
                var delta = entry.Position - pos;
                if (delta.LengthSquared() <= radiusSquared)
                {
                    results.Add(entry.Entity);
                }
            }
        }
    }

    public void GetInRadiusFast(Vector3 pos, float radius, List<Entity> results)
    {
        var radiusSquared = radius * radius;

        var cx = ToCell(pos.X);
        var cy = ToCell(pos.Y);
        var cz = ToCell(pos.Z);

        int cellSpan = (int)float.Ceiling(radius * _inverseCellSize);

        for (var x = cx - cellSpan; x <= cx + cellSpan; x++)
        for (var y = cy - cellSpan; y <= cy + cellSpan; y++)
        for (var z = cz - cellSpan; z <= cz + cellSpan; z++)
        {
            var hash = Hash(x, y, z);

            if (!_grids.TryGetValue(hash, out var cell))
                continue;

            for (var i = 0; i < cell.Count; i++)
            {
                var entry = cell[i];
                var delta = entry.Position - pos;
                if (delta.LengthSquared() <= radiusSquared)
                {
                    results.Add(entry.Entity);
                }
            }
        }
    }
}
