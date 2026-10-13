// SpatialSorter.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoKit.ArchetypeEcs.Archtypes;
using MonoKit.ArchetypeEcs.Entities;

namespace MonoKit.ArchetypeEcs;

public sealed class SpatialSorter<TPos>(float cellSize, Func<TPos, Vector3> getPosition)
    where TPos : struct
{
    private readonly RowGrid3D<TPos> _grid = new(cellSize);
    private readonly List<Archetype> _archetypes = [];
    private TPos[] _positions = [];
    private int[] _archetypeIndices = [];
    private int[] _rows = [];
    private int[] _entityIds = [];

    public bool TryBegin(World world, out SpatialField<TPos> field)
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
                _archetypeIndices[index] = archetypeIndex;
                _rows[index] = row;
                _entityIds[index] = archetype.GetEntityAt(row).Id;
                index++;
            }
        }

        var order = _grid.Prepare(_positions, index, getPosition);
        RowOrderUtility.PermuteColumn(_positions, order, index);
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
        _archetypeIndices = new int[newCapacity];
        _rows = new int[newCapacity];
        _entityIds = new int[newCapacity];
    }
}

public readonly struct SpatialField<TPos>
    where TPos : struct
{
    private readonly Archetype[] _archetypes;
    private readonly TPos[] _positions;
    private readonly int[] _archetypeIndices;
    private readonly int[] _rows;
    private readonly int[] _entityIds;
    private readonly RowGrid3D<TPos> _grid;
    private readonly Func<TPos, Vector3> _getPosition;

    public readonly int Count;

    internal SpatialField(
        Archetype[] archetypes,
        TPos[] positions,
        int[] archetypeIndices,
        int[] rows,
        int[] entityIds,
        int count,
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
        _grid = grid;
        _getPosition = getPosition;
    }

    /// <summary>The Morton-sorted positions. Indices are valid for the whole field.</summary>
    public TPos[] Positions => _positions;

    public TPos GetPosition(int index) => _positions[index];

    public Entity GetEntity(int index) => new(_entityIds[index]);

    public void GetNeighbors(int index, float radius, List<int> results) =>
        _grid.GetInRadius(_getPosition(_positions[index]), radius, results);

    public void GetNeighbors(Vector3 position, float radius, List<int> results) =>
        _grid.GetInRadius(position, radius, results);

    /// <summary>
    /// Resolves the <typeparamref name="T"/> column of every archetype in the
    /// snapshot once, so per-access lookups are plain array reads. Archetypes
    /// without the component keep a null column; check <see cref="ComponentCache{T}.Has"/>.
    /// Call this once before iterating, not per entity.
    /// </summary>
    public ComponentCache<T> Cache<T>()
        where T : struct
    {
        var columns = new T[_archetypes.Length][];
        for (var i = 0; i < _archetypes.Length; i++)
        {
            if (_archetypes[i].TryGetColumn(typeof(T), out var column))
                columns[i] = (T[])column;
        }
        return new ComponentCache<T>(columns, _archetypeIndices, _rows);
    }
}

/// <summary>
/// Per-frame random-access view on one component type over the spatial
/// snapshot. One access costs two sequential int reads, one null check and
/// one array read; there are no dictionary lookups, no boxing and no
/// per-entity record lookups.
/// </summary>
public readonly struct ComponentCache<T>
    where T : struct
{
    private readonly T[]?[] _columns;
    private readonly int[] _archetypeIndices;
    private readonly int[] _rows;

    internal ComponentCache(T[]?[] columns, int[] archetypeIndices, int[] rows)
    {
        _columns = columns;
        _archetypeIndices = archetypeIndices;
        _rows = rows;
    }

    public bool Has(int index) => _columns[_archetypeIndices[index]] != null;

    public bool TryGet(int index, out T value)
    {
        var column = _columns[_archetypeIndices[index]];
        if (column == null)
        {
            value = default;
            return false;
        }
        value = column[_rows[index]];
        return true;
    }

    /// <summary>Writable access. Check <see cref="Has"/> first when the component is optional.</summary>
    public ref T GetRef(int index)
    {
        var column = _columns[_archetypeIndices[index]];
        if (column == null)
            throw new InvalidOperationException(
                $"Component {typeof(T).Name} is not present on this entity's archetype."
            );
        return ref column[_rows[index]];
    }
}
