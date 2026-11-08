// AppHost - SpatialQueryContext.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoKit.GameRuntime.ArchetypeEcs.Entities;

namespace MonoKit.GameRuntime.ArchetypeEcs.SpatialQueries;

/// <summary>
///     Spatial neighbor queries over entities holding a <typeparamref name="TPos" /> component.
///     Call <see cref="BeginFrame" /> once per frame before any query; all query methods are
///     valid only until the next <see cref="BeginFrame" /> call. Neighbor results always
///     include the query entity itself (distance 0 passes the radius test). For symmetric
///     pair interactions, process only <c>n &gt; i</c> (or <c>n &lt; i</c>) to visit each pair once.
/// </summary>
public sealed class SpatialQueryContext<TPos> : IDisposable
    where TPos : struct
{
    private readonly Dictionary<Type, object> _componentCaches = new();
    private readonly SpatialEpoch _epoch = new();
    private readonly ConcurrentStack<List<int>> _scratchBuffers = new();
    private SpatialField<TPos>? _field;
    private int[] _generationById = [];
    private SpatialSorter<TPos>? _sorter;
    private int[] _spatialIndexById = [];

    public SpatialField<TPos> Field => GetField();

    public void Dispose()
    {
        _componentCaches.Clear();
        _spatialIndexById = [];
        _generationById = [];
        _scratchBuffers.Clear();
        _field = null;
        _sorter = null;
    }

    private List<int> RentScratchBuffer()
    {
        return !_scratchBuffers.TryPop(out var buffer) ? new List<int>(64) : buffer;
    }

    private void ReturnScratchBuffer(List<int> buffer)
    {
        buffer.Clear();
        _scratchBuffers.Push(buffer);
    }

    private void ClearEntityToSpatialIndex()
    {
        if (_spatialIndexById.Length > 0)
            Array.Fill(_spatialIndexById, -1);
    }

    private void EnsureEntityMapCapacity(int id)
    {
        if (id < _spatialIndexById.Length)
            return;

        var oldLength = _spatialIndexById.Length;
        var newLength = int.Max(id + 1, oldLength * 2);
        Array.Resize(ref _spatialIndexById, newLength);
        Array.Resize(ref _generationById, newLength);
        Array.Fill(_spatialIndexById, -1, oldLength, newLength - oldLength);
    }

    private bool TryGetSpatialIndex(Entity entity, out int index)
    {
        index = -1;
        var id = entity.Id;
        if ((uint)id >= (uint)_spatialIndexById.Length)
            return false;

        var spatialIndex = _spatialIndexById[id];
        if (spatialIndex < 0 || _generationById[id] != entity.Generation)
            return false;

        index = spatialIndex;
        return true;
    }

    public bool BeginFrame(World world, float cellSize, Func<TPos, Vector3> getPosition)
    {
        _epoch.Next();
        _componentCaches.Clear();
        ClearEntityToSpatialIndex();
        _field = null;

        if (_sorter == null || _sorter.CellSize != cellSize)
            _sorter = new SpatialSorter<TPos>(cellSize, getPosition);

        if (!_sorter.TryBegin(world, _epoch.Current, out var field))
            return false;

        _field = field;
        for (var i = 0; i < field.Count; i++)
        {
            var entity = field.GetEntity(i);
            EnsureEntityMapCapacity(entity.Id);
            _spatialIndexById[entity.Id] = i;
            _generationById[entity.Id] = entity.Generation;
        }

        return true;
    }

    public bool IsValid(SpatialField<TPos> field)
    {
        return _field is { } current && current.Version == field.Version;
    }

    private SpatialField<TPos> GetField()
    {
        if (_field is not { } field)
            throw new InvalidOperationException(
                "No spatial frame is active. Call BeginFrame first."
            );
        return field;
    }

    /// <summary>
    /// Raw all-pairs pass: the action gets spatial indices and entities for both sides, so any
    /// number of components can be pulled via <see cref="GetComponentCache{T}"/>().GetRef(index).
    /// Neighbor results include the self pair (distance 0); skip <paramref name="neighborIndex"/> ==
    /// <paramref name="selfIndex"/> or process only neighborIndex &gt; selfIndex for unordered pairs.
    /// </summary>
    public void ForEachSelfNeighborRaw(float radius, RefPairAction action)
    {
        var field = GetField();
        var count = field.Count;
        var neighbors = RentScratchBuffer();

        try
        {
            for (var i = 0; i < count; i++)
            {
                var entity = field.GetEntity(i);
                field.GetNeighbors(i, radius, neighbors);
                foreach (var n in neighbors)
                    action(i, entity, n, field.GetEntity(n));
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    /// <summary>Parallel variant of <see cref="ForEachSelfNeighborRaw"/>.</summary>
    public void ForEachSelfNeighborRawParallel(float radius, RefPairAction action)
    {
        var field = GetField();
        var count = field.Count;

        Parallel.ForEach(
            Partitioner.Create(0, count),
            RentScratchBuffer,
            (range, _, neighbors) =>
            {
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    var entity = field.GetEntity(i);
                    field.GetNeighbors(i, radius, neighbors);
                    foreach (var n in neighbors)
                        action(i, entity, n, field.GetEntity(n));
                }

                return neighbors;
            },
            ReturnScratchBuffer
        );
    }

    public void ForEachSelfNeighbor<TSelf, T1>(
        float radius,
        RefSelfNeighborAction<TSelf, T1> action
    )
        where TSelf : struct
        where T1 : struct
    {
        var field = GetField();
        var count = field.Count;
        var selfCache = GetComponentCache<TSelf>();
        var cache1 = GetComponentCache<T1>();
        var neighbors = RentScratchBuffer();

        try
        {
            for (var i = 0; i < count; i++)
            {
                if (!selfCache.Has(i))
                    continue;

                ref var self = ref selfCache.GetRef(i);
                var entity = field.GetEntity(i);
                field.GetNeighbors(i, radius, neighbors);

                foreach (var n in neighbors)
                {
                    if (!cache1.TryGet(n, out var c1))
                        continue;
                    action(entity, ref self, field.GetEntity(n), ref c1);
                }
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    public void ForEachSelfNeighbor<TSelf, T1, T2>(
        float radius,
        RefSelfNeighborAction<TSelf, T1, T2> action
    )
        where TSelf : struct
        where T1 : struct
        where T2 : struct
    {
        var field = GetField();
        var count = field.Count;
        var selfCache = GetComponentCache<TSelf>();
        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var neighbors = RentScratchBuffer();

        try
        {
            for (var i = 0; i < count; i++)
            {
                if (!selfCache.Has(i))
                    continue;

                ref var self = ref selfCache.GetRef(i);
                var entity = field.GetEntity(i);
                field.GetNeighbors(i, radius, neighbors);

                foreach (var n in neighbors)
                {
                    if (!cache1.TryGet(n, out var c1))
                        continue;
                    if (!cache2.TryGet(n, out var c2))
                        continue;
                    action(entity, ref self, field.GetEntity(n), ref c1, ref c2);
                }
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    public void ForEachSelfNeighbor<TSelf, T1, T2, T3>(
        float radius,
        RefSelfNeighborAction<TSelf, T1, T2, T3> action
    )
        where TSelf : struct
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var field = GetField();
        var count = field.Count;
        var selfCache = GetComponentCache<TSelf>();
        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var cache3 = GetComponentCache<T3>();
        var neighbors = RentScratchBuffer();

        try
        {
            for (var i = 0; i < count; i++)
            {
                if (!selfCache.Has(i))
                    continue;

                ref var self = ref selfCache.GetRef(i);
                var entity = field.GetEntity(i);
                field.GetNeighbors(i, radius, neighbors);

                foreach (var n in neighbors)
                {
                    if (!cache1.TryGet(n, out var c1))
                        continue;
                    if (!cache2.TryGet(n, out var c2))
                        continue;
                    if (!cache3.TryGet(n, out var c3))
                        continue;
                    action(entity, ref self, field.GetEntity(n), ref c1, ref c2, ref c3);
                }
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    public void ForEachSelfNeighbor<TSelf, T1, T2, T3, T4>(
        float radius,
        RefSelfNeighborAction<TSelf, T1, T2, T3, T4> action
    )
        where TSelf : struct
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var field = GetField();
        var count = field.Count;
        var selfCache = GetComponentCache<TSelf>();
        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var cache3 = GetComponentCache<T3>();
        var cache4 = GetComponentCache<T4>();
        var neighbors = RentScratchBuffer();

        try
        {
            for (var i = 0; i < count; i++)
            {
                if (!selfCache.Has(i))
                    continue;

                ref var self = ref selfCache.GetRef(i);
                var entity = field.GetEntity(i);
                field.GetNeighbors(i, radius, neighbors);

                foreach (var n in neighbors)
                {
                    if (!cache1.TryGet(n, out var c1))
                        continue;
                    if (!cache2.TryGet(n, out var c2))
                        continue;
                    if (!cache3.TryGet(n, out var c3))
                        continue;
                    if (!cache4.TryGet(n, out var c4))
                        continue;
                    action(entity, ref self, field.GetEntity(n), ref c1, ref c2, ref c3, ref c4);
                }
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    public void ForEachSelfNeighborParallel<TSelf, T1>(
        float radius,
        RefSelfNeighborAction<TSelf, T1> action
    )
        where TSelf : struct
        where T1 : struct
    {
        var field = GetField();
        var count = field.Count;
        var selfCache = GetComponentCache<TSelf>();
        var cache1 = GetComponentCache<T1>();

        Parallel.ForEach(
            Partitioner.Create(0, count),
            RentScratchBuffer,
            (range, _, neighbors) =>
            {
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    if (!selfCache.Has(i))
                        continue;

                    ref var self = ref selfCache.GetRef(i);
                    var entity = field.GetEntity(i);
                    field.GetNeighbors(i, radius, neighbors);

                    foreach (var n in neighbors)
                    {
                        if (!cache1.TryGet(n, out var c1))
                            continue;
                        action(entity, ref self, field.GetEntity(n), ref c1);
                    }
                }

                return neighbors;
            },
            ReturnScratchBuffer
        );
    }

    public void ForEachSelfNeighborParallel<TSelf, T1, T2>(
        float radius,
        RefSelfNeighborAction<TSelf, T1, T2> action
    )
        where TSelf : struct
        where T1 : struct
        where T2 : struct
    {
        var field = GetField();
        var count = field.Count;
        var selfCache = GetComponentCache<TSelf>();
        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();

        Parallel.ForEach(
            Partitioner.Create(0, count),
            RentScratchBuffer,
            (range, _, neighbors) =>
            {
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    if (!selfCache.Has(i))
                        continue;

                    ref var self = ref selfCache.GetRef(i);
                    var entity = field.GetEntity(i);
                    field.GetNeighbors(i, radius, neighbors);

                    foreach (var n in neighbors)
                    {
                        if (!cache1.TryGet(n, out var c1))
                            continue;
                        if (!cache2.TryGet(n, out var c2))
                            continue;
                        action(entity, ref self, field.GetEntity(n), ref c1, ref c2);
                    }
                }

                return neighbors;
            },
            ReturnScratchBuffer
        );
    }

    public void ForEachSelfNeighborParallel<TSelf, T1, T2, T3>(
        float radius,
        RefSelfNeighborAction<TSelf, T1, T2, T3> action
    )
        where TSelf : struct
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var field = GetField();
        var count = field.Count;
        var selfCache = GetComponentCache<TSelf>();
        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var cache3 = GetComponentCache<T3>();

        Parallel.ForEach(
            Partitioner.Create(0, count),
            RentScratchBuffer,
            (range, _, neighbors) =>
            {
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    if (!selfCache.Has(i))
                        continue;

                    ref var self = ref selfCache.GetRef(i);
                    var entity = field.GetEntity(i);
                    field.GetNeighbors(i, radius, neighbors);

                    foreach (var n in neighbors)
                    {
                        if (!cache1.TryGet(n, out var c1))
                            continue;
                        if (!cache2.TryGet(n, out var c2))
                            continue;
                        if (!cache3.TryGet(n, out var c3))
                            continue;
                        action(entity, ref self, field.GetEntity(n), ref c1, ref c2, ref c3);
                    }
                }

                return neighbors;
            },
            ReturnScratchBuffer
        );
    }

    public void ForEachSelfNeighborParallel<TSelf, T1, T2, T3, T4>(
        float radius,
        RefSelfNeighborAction<TSelf, T1, T2, T3, T4> action
    )
        where TSelf : struct
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var field = GetField();
        var count = field.Count;
        var selfCache = GetComponentCache<TSelf>();
        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var cache3 = GetComponentCache<T3>();
        var cache4 = GetComponentCache<T4>();

        Parallel.ForEach(
            Partitioner.Create(0, count),
            RentScratchBuffer,
            (range, _, neighbors) =>
            {
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    if (!selfCache.Has(i))
                        continue;

                    ref var self = ref selfCache.GetRef(i);
                    var entity = field.GetEntity(i);
                    field.GetNeighbors(i, radius, neighbors);

                    foreach (var n in neighbors)
                    {
                        if (!cache1.TryGet(n, out var c1))
                            continue;
                        if (!cache2.TryGet(n, out var c2))
                            continue;
                        if (!cache3.TryGet(n, out var c3))
                            continue;
                        if (!cache4.TryGet(n, out var c4))
                            continue;
                        action(
                            entity,
                            ref self,
                            field.GetEntity(n),
                            ref c1,
                            ref c2,
                            ref c3,
                            ref c4
                        );
                    }
                }

                return neighbors;
            },
            ReturnScratchBuffer
        );
    }

    public void ForEachNeighbor<T1>(Entity entity, float radius, RefEntityNeighborAction<T1> action)
        where T1 : struct
    {
        var field = GetField();

        if (!TryGetSpatialIndex(entity, out var index))
            return;

        var cache1 = GetComponentCache<T1>();
        var neighbors = RentScratchBuffer();

        try
        {
            field.GetNeighbors(index, radius, neighbors);

            foreach (var n in neighbors)
                if (cache1.TryGet(n, out var c1))
                    action(field.GetEntity(n), ref c1);
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    public void ForEachNeighbor<T1, T2>(
        Entity entity,
        float radius,
        RefEntityNeighborAction<T1, T2> action
    )
        where T1 : struct
        where T2 : struct
    {
        var field = GetField();

        if (!TryGetSpatialIndex(entity, out var index))
            return;

        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var neighbors = RentScratchBuffer();

        try
        {
            field.GetNeighbors(index, radius, neighbors);

            foreach (var n in neighbors)
            {
                if (!cache1.TryGet(n, out var c1))
                    continue;
                if (!cache2.TryGet(n, out var c2))
                    continue;
                action(field.GetEntity(n), ref c1, ref c2);
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    public void ForEachNeighbor<T1, T2, T3>(
        Entity entity,
        float radius,
        RefEntityNeighborAction<T1, T2, T3> action
    )
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var field = GetField();

        if (!TryGetSpatialIndex(entity, out var index))
            return;

        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var cache3 = GetComponentCache<T3>();
        var neighbors = RentScratchBuffer();

        try
        {
            field.GetNeighbors(index, radius, neighbors);

            foreach (var n in neighbors)
            {
                if (!cache1.TryGet(n, out var c1))
                    continue;
                if (!cache2.TryGet(n, out var c2))
                    continue;
                if (!cache3.TryGet(n, out var c3))
                    continue;
                action(field.GetEntity(n), ref c1, ref c2, ref c3);
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    public void ForEachNeighbor<T1, T2, T3, T4>(
        Entity entity,
        float radius,
        RefEntityNeighborAction<T1, T2, T3, T4> action
    )
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var field = GetField();

        if (!TryGetSpatialIndex(entity, out var index))
            return;

        var cache1 = GetComponentCache<T1>();
        var cache2 = GetComponentCache<T2>();
        var cache3 = GetComponentCache<T3>();
        var cache4 = GetComponentCache<T4>();
        var neighbors = RentScratchBuffer();

        try
        {
            field.GetNeighbors(index, radius, neighbors);

            foreach (var n in neighbors)
            {
                if (!cache1.TryGet(n, out var c1))
                    continue;
                if (!cache2.TryGet(n, out var c2))
                    continue;
                if (!cache3.TryGet(n, out var c3))
                    continue;
                if (!cache4.TryGet(n, out var c4))
                    continue;
                action(field.GetEntity(n), ref c1, ref c2, ref c3, ref c4);
            }
        }
        finally
        {
            ReturnScratchBuffer(neighbors);
        }
    }

    public ComponentCache<T> GetComponentCache<T>()
        where T : struct
    {
        if (_componentCaches.TryGetValue(typeof(T), out var cache))
            return (ComponentCache<T>)cache;

        var newCache = GetField().Cache<T>(_epoch);
        _componentCaches[typeof(T)] = newCache;
        return newCache;
    }
}

public delegate void RefPairAction(int selfIndex, Entity self, int neighborIndex, Entity neighbor);

public delegate void RefSelfNeighborAction<TSelf, T1>(
    Entity entity,
    ref TSelf selfComponent,
    Entity neighbor,
    ref T1 nComponent1
);

public delegate void RefSelfNeighborAction<TSelf, T1, T2>(
    Entity entity,
    ref TSelf selfComponent,
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2
);

public delegate void RefSelfNeighborAction<TSelf, T1, T2, T3>(
    Entity entity,
    ref TSelf selfComponent,
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2,
    ref T3 nComponent3
);

public delegate void RefSelfNeighborAction<TSelf, T1, T2, T3, T4>(
    Entity entity,
    ref TSelf selfComponent,
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2,
    ref T3 nComponent3,
    ref T4 nComponent4
);

public delegate void RefEntityNeighborAction<T1>(Entity neighbor, ref T1 nComponent1);

public delegate void RefEntityNeighborAction<T1, T2>(
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2
);

public delegate void RefEntityNeighborAction<T1, T2, T3>(
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2,
    ref T3 nComponent3
);

public delegate void RefEntityNeighborAction<T1, T2, T3, T4>(
    Entity neighbor,
    ref T1 nComponent1,
    ref T2 nComponent2,
    ref T3 nComponent3,
    ref T4 nComponent4
);
