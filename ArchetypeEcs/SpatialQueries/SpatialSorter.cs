// SpatialSorter.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoKit.ArchetypeEcs.Archetypes;
using MonoKit.ArchetypeEcs.Entities;

namespace MonoKit.ArchetypeEcs.SpatialQueries;

public sealed class SpatialSorter<TPos>(float cellSize, Func<TPos, Vector3> getPosition)
    where TPos : struct
{
    private readonly RowGrid3D<TPos> _grid = new(cellSize);
    private readonly List<Archetype> _archetypes = [];
    private TPos[] _positions = [];
    private Vector3[] _vecPositions = [];
    private int[] _archetypeIndices = [];
    private int[] _rows = [];
    private Entity[] _entityIds = [];

    internal float CellSize => _grid.CellSize;

    internal bool TryBegin(World world, int version, out SpatialField<TPos> field)
    {
        field = default;

        _archetypes.Clear();
        var archetypeSet = world.ArchetypeManager.GetArchetypesWith(typeof(TPos));
        if (archetypeSet != null)
            _archetypes.AddRange(archetypeSet);

        var count = 0;
        foreach (var archetype in _archetypes)
            count += archetype.RowCount;
        if (count == 0)
            return false;

        EnsureCapacity(count);

        var index = 0;
        for (var archetypeIndex = 0; archetypeIndex < _archetypes.Count; archetypeIndex++)
        {
            var archetype = _archetypes[archetypeIndex];
            if (!archetype.TryGetColumn(typeof(TPos), out var column))
                continue;
            var positions = (TPos[])column;
            for (var row = 0; row < archetype.RowCount; row++)
            {
                _positions[index] = positions[row];
                _vecPositions[index] = getPosition(positions[row]);
                _archetypeIndices[index] = archetypeIndex;
                _rows[index] = row;
                _entityIds[index] = archetype.GetEntityAt(row);
                index++;
            }
        }

        var order = _grid.Prepare(_positions, _vecPositions, index, getPosition);
        RowOrderUtility.PermuteColumn(_positions, order, index);
        RowOrderUtility.PermuteColumn(_vecPositions, order, index);
        RowOrderUtility.PermuteColumn(_archetypeIndices, order, index);
        RowOrderUtility.PermuteColumn(_rows, order, index);
        RowOrderUtility.PermuteColumn(_entityIds, order, index);
        _grid.BuildRanges(_positions, index, getPosition);

        field = new SpatialField<TPos>(
            [.. _archetypes],
            _positions,
            _archetypeIndices,
            _rows,
            _entityIds,
            index,
            version,
            _grid,
            getPosition
        );
        return true;
    }

    private void EnsureCapacity(int capacity)
    {
        if (_positions.Length >= capacity)
            return;
        var newCapacity = int.Max(capacity, _positions.Length * 2);
        _positions = new TPos[newCapacity];
        _vecPositions = new Vector3[newCapacity];
        _archetypeIndices = new int[newCapacity];
        _rows = new int[newCapacity];
        _entityIds = new Entity[newCapacity];
    }
}

public sealed class SpatialEpoch
{
    internal int Current;

    internal void Next() => Current++;
}

public readonly struct SpatialField<TPos>
    where TPos : struct
{
    private readonly Archetype[] _archetypes;
    private readonly TPos[] _positions;
    private readonly int[] _archetypeIndices;
    private readonly int[] _rows;
    private readonly Entity[] _entityIds;
    private readonly RowGrid3D<TPos> _grid;
    private readonly Func<TPos, Vector3> _getPosition;

    public readonly int Count;
    public readonly int Version;

    internal SpatialField(
        Archetype[] archetypes,
        TPos[] positions,
        int[] archetypeIndices,
        int[] rows,
        Entity[] entityIds,
        int count,
        int version,
        RowGrid3D<TPos> grid,
        Func<TPos, Vector3> getPosition
    )
    {
        _archetypes = archetypes;
        _positions = positions;
        _archetypeIndices = archetypeIndices;
        _rows = rows;
        _entityIds = entityIds;
        Count = count;
        Version = version;
        _grid = grid;
        _getPosition = getPosition;
    }

    public TPos[] Positions => _positions;

    public TPos GetPosition(int index) => _positions[index];

    public Entity GetEntity(int index) => _entityIds[index];

    public void GetNeighbors(int index, float radius, List<int> results) =>
        _grid.GetInRadius(_getPosition(_positions[index]), radius, results);

    public void GetNeighbors(Vector3 position, float radius, List<int> results) =>
        _grid.GetInRadius(position, radius, results);

    public ComponentCache<T> Cache<T>(SpatialEpoch epoch)
        where T : struct
    {
        var columns = new T[_archetypes.Length][];
        for (var i = 0; i < _archetypes.Length; i++)
        {
            if (_archetypes[i].TryGetColumn(typeof(T), out var column))
                columns[i] = (T[])column;
        }
        return new ComponentCache<T>(columns, _archetypeIndices, _rows, epoch, epoch.Current);
    }
}

public readonly struct ComponentCache<T>
    where T : struct
{
    private readonly T[]?[] _columns;
    private readonly int[] _archetypeIndices;
    private readonly int[] _rows;
    private readonly SpatialEpoch _epoch;
    private readonly int _version;

    internal ComponentCache(
        T[]?[] columns,
        int[] archetypeIndices,
        int[] rows,
        SpatialEpoch epoch,
        int version
    )
    {
        _columns = columns;
        _archetypeIndices = archetypeIndices;
        _rows = rows;
        _epoch = epoch;
        _version = version;
    }

    private void ThrowIfStale()
    {
        if (_epoch.Current != _version)
            throw new InvalidOperationException(
                $"ComponentCache<{typeof(T).Name}> is from an older spatial frame."
            );
    }

    public bool Has(int index)
    {
        ThrowIfStale();
        return _columns[_archetypeIndices[index]] != null;
    }

    public bool TryGet(int index, out T value)
    {
        ThrowIfStale();
        var column = _columns[_archetypeIndices[index]];
        if (column == null)
        {
            value = default;
            return false;
        }
        value = column[_rows[index]];
        return true;
    }

    public ref T GetRef(int index)
    {
        ThrowIfStale();
        var column = _columns[_archetypeIndices[index]];
        if (column == null)
            throw new InvalidOperationException(
                $"Component {typeof(T).Name} is not present on this entity's archetype."
            );
        return ref column[_rows[index]];
    }
}
