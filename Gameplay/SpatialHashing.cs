// SpatialHashing.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using MonoKit.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.Gameplay
{
    public class SpatialHashing(int cellSize)
    {
        public int Count => _hashes.Count;
        public readonly int CellSize = cellSize;
        private readonly ConcurrentDictionary<(int, int), SpatialGrid> _grids = new();
        private readonly ConcurrentDictionary<GameObject, List<(int, int)>> _hashes = new();

        public void Add(GameObject obj)
        {
            var start = Vector2.Floor(obj.BoundBox.ToRectangleF().TopLeft / CellSize);
            var end = Vector2.Ceiling(obj.BoundBox.ToRectangleF().BottomRight / CellSize);
            for (var x = start.X; x < end.X; x++)
            {
                for (var y = start.Y; y < end.Y; y++)
                {
                    var hash = ((int)x, (int)y);
                    _grids.GetOrAdd(hash, _ => new(hash, CellSize)).Add(obj);
                    _hashes.GetOrAdd(obj, _ => new()).Add(hash);
                }
            }
        }

        public void Remove(GameObject obj)
        {
            var hashes = _hashes[obj];
            foreach (var hash in hashes)
            {
                if (!_grids.TryGetValue(hash, out var grid)) continue;
                grid.Remove(obj);
                if (grid.IsEmpty)
                    _grids.TryRemove(hash, out _);
            }
            _hashes.TryRemove(obj, out _);
        }

        public void Rearrange()
        {
            var movingObjects = _hashes
                .Where(kvp => kvp.Key.Velocity != 0)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var obj in movingObjects)
            {
                Remove(obj);
                Add(obj);
            }
        }


        public void Clear() => _grids.Clear();

        public (int, int) Hash(Vector2 vector)
            => ((int)float.Floor(vector.X / CellSize), (int)float.Floor(vector.Y / CellSize));

        public void GetObjectsInRadius<T>(Vector2 position, float radius, List<T> objectsInRadius, bool sortedByDistance = true) where T : GameObject
        {
            var startX = (int)MathF.Floor((position.X - radius) / CellSize);
            var endX = (int)MathF.Ceiling((position.X + radius) / CellSize);
            var startY = (int)MathF.Floor((position.Y - radius) / CellSize);
            var endY = (int)MathF.Ceiling((position.Y + radius) / CellSize);
            var lookUpCircle = new CircleF(position, radius);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    var hash = (x, y);
                    if (!_grids.TryGetValue(hash, out var grid)) continue;
                    grid.AddObjectsInCircle(lookUpCircle, ref objectsInRadius);
                }
            }

            if (!sortedByDistance) return;
            objectsInRadius.AsParallel().OrderBy(obj => Vector2.Distance(position, obj.Position) - obj.BoundBox.Radius);
        }

        public void GetObjectsInRectangle<T>(RectangleF searchRectangle, List<T> objectsInRectangle) where T : GameObject
        {
            int startX = (int)Math.Floor(searchRectangle.Left / CellSize);
            int endX = (int)Math.Ceiling(searchRectangle.Right / CellSize);
            int startY = (int)Math.Floor(searchRectangle.Top / CellSize);
            int endY = (int)Math.Ceiling(searchRectangle.Bottom / CellSize);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    var hash = (x, y);
                    if (!_grids.TryGetValue(hash, out var grid)) continue;
                    grid.AddObjectsInRectangle(searchRectangle, ref objectsInRectangle);
                }
            }
        }

        public void GetObjectsInBucket<T>(Vector2 position, List<T> objectsInBucket) where T : GameObject
        {
            var hash = Hash(position);
            if (!_grids.TryGetValue(hash, out var bucket)) return;
            bucket.AddObjects(ref objectsInBucket);
        }

        public List<T> GetObjectsInRadius<T>(Vector2 position, float radius, bool sortedByDistance = true) where T : GameObject
        {
            List<T> objectsInRadius = new List<T>();
            GetObjectsInRadius(position, radius, objectsInRadius, sortedByDistance);
            return objectsInRadius;
        }

        public List<T> GetObjectsInRectangle<T>(RectangleF searchRectangle) where T : GameObject
        {
            List<T> objectsInRadius = new();
            GetObjectsInRectangle(searchRectangle, objectsInRadius);
            return objectsInRadius;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 lookUpPosition, float cameraZoom)
        {
            foreach (var grid in _grids.Values)
                grid.Draw(spriteBatch, Color.Red, cameraZoom);

            int radius = 100;
            int radius2 = radius * 2;
            RectangleF rectangle = new(lookUpPosition - new Vector2(radius), new(radius2, radius2));
            var gameObjects = new List<GameObject>();
            GetObjectsInBucket(lookUpPosition, gameObjects);
            foreach (GameObject obj in gameObjects)
                spriteBatch.DrawLine(lookUpPosition, obj.Position, Color.White, 1f / cameraZoom, 0.9f);
            gameObjects.Clear();
            spriteBatch.DrawRectangleF(rectangle, Color.Red, cameraZoom);
            GetObjectsInRectangle(rectangle, gameObjects);
            foreach (GameObject obj in gameObjects)
                spriteBatch.DrawLine(lookUpPosition, obj.Position, Color.Red, 1f / cameraZoom, 0.99f);

            gameObjects.Clear();
            spriteBatch.DrawCircleF(lookUpPosition, radius, Color.Orange, cameraZoom);
            GetObjectsInRadius(lookUpPosition, radius, gameObjects);
            foreach (GameObject obj in gameObjects)
                spriteBatch.DrawLine(lookUpPosition, obj.Position, Color.Orange, 1f / cameraZoom, 0.999f);
        }
    }
}