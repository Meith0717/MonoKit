// SpatialGrid.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Content;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.SpatialManagement
{
    internal class SpatialGrid((int, int) hash, int size)
    {
        private readonly (int, int) _hash = hash;
        public readonly RectangleF Bounds = new(hash.Item1 * size, hash.Item2 * size, size, size);
        private readonly HashSet<ISpatial> _objects = new();
        private readonly object _lock = new();

        public ISpatial[] Objects => _objects.ToArray();

        public bool IsEmpty
        {
            get
            {
                lock (_lock)
                    return _objects.Count == 0;
            }
        }

        public void Add(ISpatial item)
        {
            lock (_lock)
                _objects.Add(item);
        }

        public void Remove(ISpatial item)
        {
            lock (_lock)
                _objects.Remove(item);
        }

        public void AddObjectsInCircle<T>(CircleF circleF, ref List<T> values)
        {
            lock (_lock)
            {
                var objects = _objects.ToArray();
                for (int i = 0; i < objects.Length; i++)
                {
                    var obj = objects[i];
                    if (obj is not T)
                        continue;

                    if (circleF.Intersects(obj.Bounding))
                        values.Add((T)obj);
                }
            }
        }

        public void AddObjectsInRectangle<T>(RectangleF rectangle, ref List<T> values)
        {
            lock (_lock)
            {
                var objects = _objects.ToArray();
                for (int i = 0; i < objects.Length; i++)
                {
                    var obj = objects[i];
                    if (obj is not T)
                        continue;

                    if (rectangle.Intersects(obj.Bounding))
                        values.Add((T)obj);
                }
            }
        }

        public void AddObjects<T>(ref List<T> values)
        {
            lock (_lock)
            {
                var objects = _objects.ToArray();
                for (int i = 0; i < objects.Length; i++)
                {
                    var obj = objects[i];
                    if (obj is not T)
                        continue;

                    values.Add((T)obj);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float cameraZoom)
        {
            var font = ContentProvider.Container<SpriteFont>().Get("default_font");
            var pos = Bounds.TopRight - new Vector2(10, -10);
            var str = $"({_hash.Item1},{_hash.Item2})";
            var size = font.MeasureString(str) * .2f;
            pos.X -= size.X;
            spriteBatch.DrawString(font, str, pos, color, 0, Vector2.Zero, .2f, SpriteEffects.None, 1);
            spriteBatch.DrawRectangle(Bounds, color, 2f / cameraZoom);
            pos = Bounds.Position + new Vector2(10, 10);
            foreach (var obj in _objects)
            {
                spriteBatch.DrawString(font, $"({obj.GetType().Name})", pos, color, 0, Vector2.Zero, .2f, SpriteEffects.None, 1);
                pos.Y += 30;
            }
        }
    }
}