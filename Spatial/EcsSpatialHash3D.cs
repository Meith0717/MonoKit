using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace MonoKit.Spatial;

public sealed class EcsSpatialHash3D
{
    private readonly Dictionary<long, List<Entity>> _grids;
    private readonly List<List<Entity>> _activeCells;

    private readonly float _inverseCellSize;

    public readonly int CellSize;

    public EcsSpatialHash3D(int cellSize, int capacity = 1024)
    {
        CellSize = cellSize;
        _inverseCellSize = 1f / cellSize;

        _grids = new Dictionary<long, List<Entity>>(capacity);
        _activeCells = new List<List<Entity>>(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long Hash(int x, int y, int z)
    {
        return ((long)x * 73856093) ^ ((long)y * 19349663) ^ ((long)z * 83492791);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ToCell(float value)
    {
        return (int)float.Floor(value * _inverseCellSize);
    }

    public void Clear()
    {
        foreach (var cell in _activeCells)
            cell.Clear();

        _activeCells.Clear();
        _grids.Clear();
    }

    public void Add(Entity entity, Vector3 position, Vector3 size)
    {
        int startX = ToCell(position.X);
        int startY = ToCell(position.Y);
        int startZ = ToCell(position.Z);

        int endX;
        int endY;
        int endZ;

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

        for (int x = startX; x < endX; x++)
        for (int y = startY; y < endY; y++)
        for (int z = startZ; z < endZ; z++)
        {
            long hash = Hash(x, y, z);

            if (!_grids.TryGetValue(hash, out var cell))
            {
                cell = new List<Entity>(8);
                _grids.Add(hash, cell);
            }

            if (cell.Count == 0)
                _activeCells.Add(cell);

            cell.Add(entity);
        }
    }

    public void GetInRadius(Vector3 pos, float radius, List<Entity> results)
    {
        int startX = ToCell(pos.X - radius);
        int endX = ToCell(pos.X + radius) + 1;

        int startY = ToCell(pos.Y - radius);
        int endY = ToCell(pos.Y + radius) + 1;

        int startZ = ToCell(pos.Z - radius);
        int endZ = ToCell(pos.Z + radius) + 1;

        for (int x = startX; x < endX; x++)
        for (int y = startY; y < endY; y++)
        for (int z = startZ; z < endZ; z++)
        {
            long hash = Hash(x, y, z);

            if (_grids.TryGetValue(hash, out var cell))
                results.AddRange(cell);
        }
    }

    public void GetInBox(BoundingBox box, List<Entity> results)
    {
        int startX = ToCell(box.Min.X);
        int endX = ToCell(box.Max.X) + 1;

        int startY = ToCell(box.Min.Y);
        int endY = ToCell(box.Max.Y) + 1;

        int startZ = ToCell(box.Min.Z);
        int endZ = ToCell(box.Max.Z) + 1;

        for (int x = startX; x < endX; x++)
        for (int y = startY; y < endY; y++)
        for (int z = startZ; z < endZ; z++)
        {
            long hash = Hash(x, y, z);

            if (_grids.TryGetValue(hash, out var cell))
                results.AddRange(cell);
        }
    }
}
