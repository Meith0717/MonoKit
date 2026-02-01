// EcsSpatialHash.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MonoKit.Spatial;

public class EcsSpatialHash(int cellSize)
{
    private readonly Dictionary<(int, int), List<int>> _grids = new();
    private readonly Dictionary<int, List<(int, int)>> _entityHashes = new();
    public readonly int CellSize = cellSize;

    public void UpdateEntity(int entityId, Vector2 position, int width, int height)
    {
        if (_entityHashes.TryGetValue(entityId, out var oldHashes))
        {
            foreach (var hash in oldHashes)
            {
                if (_grids.TryGetValue(hash, out var cell))
                {
                    cell.Remove(entityId);
                    if (cell.Count == 0)
                        _grids.Remove(hash);
                }
            }
            oldHashes.Clear();
        }
        else
            _entityHashes[entityId] = [];

        var bottomRight = position + new Vector2(width, height);
        var start = Vector2.Floor(position / CellSize);
        var end = Vector2.Ceiling(bottomRight / CellSize);

        for (var x = (int)start.X; x < (int)end.X; x++)
        for (var y = (int)start.Y; y < (int)end.Y; y++)
        {
            var hash = (x, y);
            if (!_grids.ContainsKey(hash))
                _grids[hash] = new List<int>();

            _grids[hash].Add(entityId);
            _entityHashes[entityId].Add(hash);
        }
    }

    public void RemoveEntity(int entityId)
    {
        if (!_entityHashes.Remove(entityId, out var hashes))
            return;

        foreach (var hash in hashes)
        {
            if (_grids.TryGetValue(hash, out var cell))
            {
                cell.Remove(entityId);
                if (cell.Count == 0)
                    _grids.Remove(hash);
            }
        }
    }

    public void GetInRadius(Vector2 pos, float radius, List<int> results)
    {
        var startX = (int)float.Floor((pos.X - radius) / CellSize);
        var endX = (int)float.Ceiling((pos.X + radius) / CellSize);
        var startY = (int)float.Floor((pos.Y - radius) / CellSize);
        var endY = (int)float.Ceiling((pos.Y + radius) / CellSize);

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        {
            if (_grids.TryGetValue((x, y), out var cell))
                results.AddRange(cell);
        }
    }

    public void DrawDebug(SpriteBatch spriteBatch, Color color, float thickness = 1f)
    {
        var rect = new RectangleF(0, 0, CellSize, CellSize);
        foreach (var ((x, y), _) in _grids)
        {
            rect.Position = new Vector2(x, y) * CellSize;
            spriteBatch.DrawRectangle(rect, color, thickness);
        }
    }
}
