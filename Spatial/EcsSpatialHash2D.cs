using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Ecs.Entities;

namespace MonoKit.Spatial;

public class EcsSpatialHash2D(int cellSize)
{
    private readonly Dictionary<(int, int), List<Entity>> _grids = new();
    private readonly Dictionary<Entity, List<(int, int)>> _entityHashes = new();
    public readonly int CellSize = cellSize;

    public void Clear()
    {
        _grids.Clear();
        _entityHashes.Clear();
    }

    public void UpdateEntity(Entity entity, Vector2 position, int width, int height)
    {
        if (_entityHashes.TryGetValue(entity, out var oldHashes))
        {
            foreach (var hash in oldHashes)
            {
                if (!_grids.TryGetValue(hash, out var cell))
                    continue;

                cell.Remove(entity);
                if (cell.Count == 0)
                    _grids.Remove(hash);
            }
            oldHashes.Clear();
        }
        else
            _entityHashes[entity] = [];

        var bottomRight = position + new Vector2(width, height);
        var start = Vector2.Floor(position / CellSize);
        var end = Vector2.Ceiling(bottomRight / CellSize);

        for (var x = (int)start.X; x < (int)end.X; x++)
        for (var y = (int)start.Y; y < (int)end.Y; y++)
        {
            var hash = (x, y);
            if (!_grids.ContainsKey(hash))
                _grids[hash] = [];

            _grids[hash].Add(entity);
            _entityHashes[entity].Add(hash);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if (!_entityHashes.Remove(entity, out var hashes))
            return;

        foreach (var hash in hashes)
        {
            if (!_grids.TryGetValue(hash, out var cell))
                continue;

            cell.Remove(entity);
            if (cell.Count == 0)
                _grids.Remove(hash);
        }
    }

    public void GetInRadius(Vector2 pos, float radius, List<Entity> results)
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

    public void GetInRectangle(RectangleF searchRectangle, List<Entity> results)
    {
        var startX = (int)float.Floor(searchRectangle.Left / CellSize);
        var endX = (int)float.Ceiling(searchRectangle.Right / CellSize);
        var startY = (int)float.Floor(searchRectangle.Top / CellSize);
        var endY = (int)float.Ceiling(searchRectangle.Bottom / CellSize);

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
            if (_grids.TryGetValue((x, y), out var cell))
                results.AddRange(cell);
    }

    public void DrawDebug(SpriteBatch spriteBatch, Color color, float thickness)
    {
        var rect = new RectangleF(0, 0, CellSize, CellSize);
        foreach (var ((x, y), _) in _grids)
        {
            rect.Position = new Vector2(x, y) * CellSize;
            spriteBatch.DrawRectangle(rect, color, thickness);
        }
    }
}
