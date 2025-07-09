// SpatialHashing.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Gameplay
{
    /// <summary>
    /// Manages spatial partitioning of GameObjects using a hash grid for efficient queries.
    /// </summary>
    public class SpatialHashing(int cellSize)
    {
        /// <summary>
        /// Gets the total number of objects currently hashed.
        /// </summary>
        public int Count => _hashes.Count;

        /// <summary>
        /// The size (in world units) of each grid cell.
        /// </summary>
        public readonly int CellSize = cellSize;

        private readonly ConcurrentDictionary<(int, int), SpatialGrid> _grids = new();
        private readonly ConcurrentDictionary<GameObject, List<(int, int)>> _hashes = new();

        /// <summary>
        /// Adds a GameObject to the spatial hash.
        /// </summary>
        /// <param name="obj">The GameObject to insert.</param>
        public void Add(GameObject obj)
        {
            var newHashes = ComputeHashes(obj);
            foreach (var hash in newHashes)
            {
                _grids.GetOrAdd(hash, _ => new(hash, CellSize)).Add(obj);
            }
            _hashes[obj] = new List<(int, int)>(newHashes);
        }

        /// <summary>
        /// Removes a GameObject from all grid cells it occupies.
        /// </summary>
        /// <param name="obj">The GameObject to remove.</param>
        public void RemoveObject(GameObject obj)
        {
            if (!_hashes.TryRemove(obj, out var oldHashes))
                return;

            foreach (var hash in oldHashes)
            {
                if (_grids.TryGetValue(hash, out var grid))
                {
                    grid.Remove(obj);
                    if (grid.IsEmpty)
                        _grids.TryRemove(hash, out _);
                }
            }
        }

        /// <summary>
        /// Recalculates cell membership for every object, updating only those that moved.
        /// </summary>
        public void Rearrange()
        {
            var objects = _hashes.Keys.ToArray();
            Parallel.ForEach(objects, obj =>
            {
                UpdateObject(obj);
            });
        }

        /// <summary>
        /// Clears all hashed objects and grids.
        /// </summary>
        public void Clear() => _grids.Clear();

        /// <summary>
        /// Computes the grid cell coordinates for a given position.
        /// </summary>
        /// <param name="vector">World-space position.</param>
        /// <returns>Tuple of cell X and Y indices.</returns>
        public (int, int) Hash(Vector2 vector)
            => ((int)float.Floor(vector.X / CellSize), (int)float.Floor(vector.Y / CellSize));

        /// <summary>
        /// Populates a list with objects within a circular area.
        /// </summary>
        /// <typeparam name="T">Type of GameObject.</typeparam>
        /// <param name="position">Center of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="objectsInRadius">Reference to the result list.</param>
        /// <param name="sortedByDistance">Whether to sort results by distance.</param>
        public void GetObjectsInRadius<T>(Vector2 position, float radius, ref List<T> objectsInRadius, bool sortedByDistance = true) where T : GameObject
        {
            var startX = (int)MathF.Floor((position.X - radius) / CellSize);
            var endX = (int)MathF.Ceiling((position.X + radius) / CellSize);
            var startY = (int)MathF.Floor((position.Y - radius) / CellSize);
            var endY = (int)MathF.Ceiling((position.Y + radius) / CellSize);
            var lookUpCircle = new CircleF(position, radius);

            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                {
                    var hash = (x, y);
                    if (!_grids.TryGetValue(hash, out var grid)) continue;
                    grid.AddObjectsInCircle(lookUpCircle, ref objectsInRadius);
                }

            if (!sortedByDistance) return;
            objectsInRadius = objectsInRadius
                .AsParallel()
                .OrderBy(obj => Vector2.Distance(position, obj.Position) - obj.BoundBox.Radius)
                .ToList();
        }

        /// <summary>
        /// Populates a list with objects within an axis-aligned rectangle.
        /// </summary>
        /// <typeparam name="T">Type of GameObject.</typeparam>
        /// <param name="searchRectangle">The rectangle to search.</param>
        /// <param name="objectsInRectangle">Reference to the result list.</param>
        public void GetObjectsInRectangle<T>(RectangleF searchRectangle, ref List<T> objectsInRectangle) where T : GameObject
        {
            int startX = (int)Math.Floor(searchRectangle.Left / CellSize);
            int endX = (int)Math.Ceiling(searchRectangle.Right / CellSize);
            int startY = (int)Math.Floor(searchRectangle.Top / CellSize);
            int endY = (int)Math.Ceiling(searchRectangle.Bottom / CellSize);

            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                {
                    var hash = (x, y);
                    if (!_grids.TryGetValue(hash, out var grid)) continue;
                    grid.AddObjectsInRectangle(searchRectangle, ref objectsInRectangle);
                }
        }

        /// <summary>
        /// Retrieves all objects in the single grid cell containing the position.
        /// </summary>
        /// <typeparam name="T">Type of GameObject.</typeparam>
        /// <param name="position">Position to look up.</param>
        /// <param name="objectsInBucket">Reference to the result list.</param>
        public void GetObjectsInBucket<T>(Vector2 position, ref List<T> objectsInBucket) where T : GameObject
        {
            var hash = Hash(position);
            if (!_grids.TryGetValue(hash, out var bucket)) return;
            bucket.AddObjects(ref objectsInBucket);
        }

        /// <summary>
        /// Convenience: returns a list of objects within a radius.
        /// </summary>
        public List<T> GetObjectsInRadius<T>(Vector2 position, float radius, bool sortedByDistance = true) where T : GameObject
        {
            var objectsInRadius = new List<T>();
            GetObjectsInRadius(position, radius, ref objectsInRadius, sortedByDistance);
            return objectsInRadius;
        }

        /// <summary>
        /// Convenience: returns a list of objects within a rectangle.
        /// </summary>
        public List<T> GetObjectsInRectangle<T>(RectangleF searchRectangle) where T : GameObject
        {
            var objectsInRectangle = new List<T>();
            GetObjectsInRectangle(searchRectangle, ref objectsInRectangle);
            return objectsInRectangle;
        }

        /// <summary>
        /// Renders the spatial grid and debug lookup areas.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch for drawing lines and shapes.</param>
        /// <param name="lookUpPosition">Center point for debug lookups.</param>
        /// <param name="cameraZoom">Current zoom factor.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 lookUpPosition, float cameraZoom)
        {
            foreach (var grid in _grids.Values)
                grid.Draw(spriteBatch, Color.Red, cameraZoom);

            int radius = 100;
            int diameter = radius * 2;
            var rectangle = new RectangleF(lookUpPosition - new Vector2(radius), new(diameter, diameter));
            var gameObjects = new List<GameObject>();

            // Draw bucket connections in white
            GetObjectsInBucket(lookUpPosition, ref gameObjects);
            foreach (var obj in gameObjects)
                spriteBatch.DrawLine(lookUpPosition, obj.Position, Color.White, 2f / cameraZoom, 0.9f);
            gameObjects.Clear();

            // Draw rectangle connections in red
            spriteBatch.DrawRectangleF(rectangle, Color.Red, cameraZoom);
            GetObjectsInRectangle(rectangle, ref gameObjects);
            foreach (var obj in gameObjects)
                spriteBatch.DrawLine(lookUpPosition, obj.Position, Color.Red, 2f / cameraZoom, 0.99f);
            gameObjects.Clear();

            // Draw circle connections in orange
            spriteBatch.DrawCircleF(lookUpPosition, radius, Color.Orange, cameraZoom);
            GetObjectsInRadius(lookUpPosition, radius, ref gameObjects);
            foreach (var obj in gameObjects)
                spriteBatch.DrawLine(lookUpPosition, obj.Position, Color.Orange, 2f / cameraZoom, 0.999f);
        }

        #region Internal Helpers

        /// <summary>
        /// Computes which grid cells an object's bounding box intersects.
        /// </summary>
        /// <param name="obj">The GameObject whose bounds are used.</param>
        /// <returns>List of cell coordinate tuples.</returns>
        private List<(int, int)> ComputeHashes(GameObject obj)
        {
            var start = Vector2.Floor(obj.BoundBox.ToRectangleF().TopLeft / CellSize);
            var end = Vector2.Ceiling(obj.BoundBox.ToRectangleF().BottomRight / CellSize);
            var hashes = new List<(int, int)>();

            for (var x = start.X; x < end.X; x++)
                for (var y = start.Y; y < end.Y; y++)
                    hashes.Add(((int)x, (int)y));

            return hashes;
        }

        /// <summary>
        /// Updates a single object’s grid membership if it moved since the last hash.
        /// </summary>
        /// <param name="obj">The GameObject to update.</param>
        private void UpdateObject(GameObject obj)
        {
            if (!_hashes.TryGetValue(obj, out var oldHashes))
            {
                Add(obj);
                return;
            }

            var newHashes = ComputeHashes(obj);
            if (newHashes.SequenceEqual(oldHashes))
                return;

            var toRemove = oldHashes.Except(newHashes);
            var toAdd = newHashes.Except(oldHashes);

            foreach (var hash in toRemove)
            {
                if (_grids.TryGetValue(hash, out var grid))
                {
                    grid.Remove(obj);
                    if (grid.IsEmpty)
                        _grids.TryRemove(hash, out _);
                }
            }

            foreach (var hash in toAdd)
                _grids.GetOrAdd(hash, _ => new(hash, CellSize)).Add(obj);

            _hashes[obj] = new List<(int, int)>(newHashes);
        }

        #endregion
    }
}
