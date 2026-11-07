// MonoKit - RowGrid3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace MonoKit.GameRuntime.ArchetypeEcs.SpatialQueries;

internal sealed class RowGrid3D<TPos>
    where TPos : struct
{
    private readonly float _inverseCellSize;
    private readonly Dictionary<long, CellRange> _ranges = new();
    private long[] _keys = [];
    private int[] _order = [];

    private Vector3[] _vecPositions = [];

    public RowGrid3D(float cellSize)
    {
        if (cellSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        _inverseCellSize = 1f / cellSize;
        CellSize = cellSize;
    }

    public float CellSize { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static long MortonEncode(int x, int y, int z)
    {
        unchecked
        {
            var result = 0L;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static long CellKey(int x, int y, int z)
    {
        return ((long)(uint)(x & 0x1F_FFFF) << 42)
            | ((long)(uint)(y & 0x1F_FFFF) << 21)
            | (uint)(z & 0x1F_FFFF);
    }

    public int[] Prepare(
        TPos[] positions,
        Vector3[] vecPositions,
        int rowCount,
        Func<TPos, Vector3> getPosition
    )
    {
        _vecPositions = vecPositions;

        if (_keys.Length < rowCount)
        {
            var capacity = int.Max(rowCount, _keys.Length * 2);
            _keys = new long[capacity];
            _order = new int[capacity];
        }

        for (var i = 0; i < rowCount; i++)
        {
            var p = getPosition(positions[i]);
            _keys[i] = MortonEncode(ToCell(p.X), ToCell(p.Y), ToCell(p.Z));
            _order[i] = i;
        }

        Array.Sort(_keys, _order, 0, rowCount);
        return _order;
    }

    public void BuildRanges(TPos[] positions, int rowCount, Func<TPos, Vector3> getPosition)
    {
        _ranges.Clear();
        if (rowCount == 0)
            return;

        var start = 0;
        var p = getPosition(positions[0]);
        var cx = ToCell(p.X);
        var cy = ToCell(p.Y);
        var cz = ToCell(p.Z);

        for (var i = 1; i < rowCount; i++)
        {
            p = getPosition(positions[i]);
            var x = ToCell(p.X);
            var y = ToCell(p.Y);
            var z = ToCell(p.Z);
            if (x == cx && y == cy && z == cz)
                continue;

            _ranges[CellKey(cx, cy, cz)] = new CellRange(start, i - start);
            (cx, cy, cz) = (x, y, z);
            start = i;
        }

        _ranges[CellKey(cx, cy, cz)] = new CellRange(start, rowCount - start);
    }

    public void GetInRadius(Vector3 center, float radius, List<int> results)
    {
        results.Clear();

        var radiusSq = radius * radius;
        var vecPositions = _vecPositions;

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
            if (!_ranges.TryGetValue(CellKey(x, y, z), out var range))
                continue;
            for (var i = 0; i < range.Count; i++)
            {
                var index = range.Start + i;
                var p = vecPositions[index];
                var dx = p.X - center.X;
                var dy = p.Y - center.Y;
                var dz = p.Z - center.Z;
                if (dx * dx + dy * dy + dz * dz < radiusSq)
                    results.Add(index);
            }
        }
    }

    private struct CellRange(int start, int count)
    {
        public readonly int Start = start;
        public readonly int Count = count;
    }
}
