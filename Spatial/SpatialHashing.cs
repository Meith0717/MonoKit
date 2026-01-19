// SpatialHashing.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Core.Extensions;

namespace MonoKit.Spatial;

public class SpatialHashing(int cellSize)
{
    private readonly ConcurrentDictionary<(int, int), SpatialGrid> _grids = new();
    private readonly ConcurrentDictionary<ISpatial, List<(int, int)>> _hashes = new();
    public readonly int CellSize = cellSize;
    public int Count => _hashes.Count;

    public void Add(ISpatial obj)
    {
        var start = Vector2.Floor(obj.Bounding.TopLeft / CellSize);
        var end = Vector2.Ceiling(obj.Bounding.BottomRight / CellSize);
        for (var x = start.X; x < end.X; x++)
        for (var y = start.Y; y < end.Y; y++)
        {
            var hash = ((int)x, (int)y);
            _grids.GetOrAdd(hash, _ => new SpatialGrid(hash, CellSize)).Add(obj);
            _hashes.GetOrAdd(obj, _ => new List<(int, int)>()).Add(hash);
        }
    }

    public void Remove(ISpatial obj)
    {
        var hashes = _hashes[obj];
        foreach (var hash in hashes)
        {
            if (_grids.TryGetValue(hash, out var grid))
                grid.Remove(obj);
            if (grid.IsEmpty)
                _grids.TryRemove(hash, out _);
        }

        _hashes.TryRemove(obj, out _);
    }

    public void Rearrange()
    {
        var movingObjects = _hashes
            .Where(kvp => kvp.Key.HasPositionChanged())
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var obj in movingObjects)
        {
            Remove(obj);
            Add(obj);
        }
    }

    public void Clear()
    {
        _grids.Clear();
    }

    public (int, int) Hash(Vector2 vector)
    {
        vector *= CellSize;
        vector.Floor();

        var x = (int)vector.X;
        var y = (int)vector.Y;

        return (x, y);
    }

    public void GetInRadius<T>(
        Vector2 position,
        float radius,
        List<T> objectsInRadius,
        bool sortedByDistance = true
    )
        where T : ISpatial
    {
        var startX = (int)MathF.Floor((position.X - radius) / CellSize);
        var endX = (int)MathF.Ceiling((position.X + radius) / CellSize);
        var startY = (int)MathF.Floor((position.Y - radius) / CellSize);
        var endY = (int)MathF.Ceiling((position.Y + radius) / CellSize);
        var lookUpCircle = new CircleF(position, radius);

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
            if (_grids.TryGetValue((x, y), out var grid))
                grid.AddObjectsInCircle(lookUpCircle, ref objectsInRadius);

        if (sortedByDistance)
            objectsInRadius
                .AsParallel()
                .OrderBy(obj =>
                    Vector2.Distance(position, obj.Position)
                    - float.Max(obj.Bounding.Height, obj.Bounding.Width)
                );
    }

    public void GetInRectangle<T>(RectangleF searchRectangle, List<T> objectsInRectangle)
        where T : ISpatial
    {
        var startX = (int)Math.Floor(searchRectangle.Left / CellSize);
        var endX = (int)Math.Ceiling(searchRectangle.Right / CellSize);
        var startY = (int)Math.Floor(searchRectangle.Top / CellSize);
        var endY = (int)Math.Ceiling(searchRectangle.Bottom / CellSize);

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
            if (_grids.TryGetValue((x, y), out var grid))
                grid.AddObjectsInRectangle(searchRectangle, ref objectsInRectangle);
    }

    public void GetInBucket<T>(Vector2 position, List<T> objectsInBucket)
        where T : ISpatial
    {
        if (_grids.TryGetValue(Hash(position), out var bucket))
            bucket.AddObjects(ref objectsInBucket);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 lookUpPosition, float cameraZoom)
    {
        foreach (var grid in _grids.Values)
            grid.Draw(spriteBatch, Color.Red, cameraZoom);

        var radius = 100;
        var radius2 = radius * 2;
        RectangleF rectangle = new(
            lookUpPosition - new Vector2(radius),
            new SizeF(radius2, radius2)
        );
        var gameObjects = new List<ISpatial>();
        GetInBucket(lookUpPosition, gameObjects);
        foreach (var obj in gameObjects)
            spriteBatch.DrawLine(lookUpPosition, obj.Position, Color.White, 1f / cameraZoom, 0.9f);
        gameObjects.Clear();
        spriteBatch.DrawRectangleF(rectangle, Color.Red, cameraZoom);
        GetInRectangle(rectangle, gameObjects);
        foreach (var obj in gameObjects)
            spriteBatch.DrawLine(lookUpPosition, obj.Position, Color.Red, 1f / cameraZoom, 0.99f);

        gameObjects.Clear();
        spriteBatch.DrawCircleF(lookUpPosition, radius, Color.Orange, cameraZoom);
        GetInRadius(lookUpPosition, radius, gameObjects);
        foreach (var obj in gameObjects)
            spriteBatch.DrawLine(
                lookUpPosition,
                obj.Position,
                Color.Orange,
                1f / cameraZoom,
                0.999f
            );
    }
}
