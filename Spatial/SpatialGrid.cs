// SpatialGrid.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Content;

namespace MonoKit.Spatial;

internal class SpatialGrid((int, int) hash, int size)
{
    private static readonly SpriteFont Font = ContentProvider
        .Container<SpriteFont>()
        .Get("default_font");
    private readonly string _id = $"ID: ({hash.Item1},{hash.Item2})";

    private readonly object _lock = new();
    private readonly HashSet<ISpatial> _objects = new();
    public readonly RectangleF Bounds = new(hash.Item1 * size, hash.Item2 * size, size, size);

    public ISpatial[] Objects => _objects.ToArray();

    public bool IsEmpty
    {
        get
        {
            lock (_lock)
            {
                return _objects.Count == 0;
            }
        }
    }

    public void Add(ISpatial item)
    {
        lock (_lock)
        {
            _objects.Add(item);
        }
    }

    public void Remove(ISpatial item)
    {
        lock (_lock)
        {
            _objects.Remove(item);
        }
    }

    public void AddObjectsInCircle<T>(CircleF circleF, ref List<T> values)
    {
        lock (_lock)
        {
            var objects = _objects.ToArray();
            for (var i = 0; i < objects.Length; i++)
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
            for (var i = 0; i < objects.Length; i++)
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
            for (var i = 0; i < objects.Length; i++)
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
        var position = Bounds.Position + new Vector2(10, 10);
        spriteBatch.DrawString(
            Font,
            _id,
            position,
            color,
            0,
            Vector2.Zero,
            .2f,
            SpriteEffects.None,
            1
        );

        position.Y += 25;
        spriteBatch.DrawString(
            Font,
            $"Amount: {_objects.Count}",
            position,
            color,
            0,
            Vector2.Zero,
            .2f,
            SpriteEffects.None,
            1
        );

        var bounds = Bounds;
        bounds.Inflate(.5f, .5f);
        spriteBatch.DrawRectangle(bounds, color, 1 / cameraZoom);
    }
}
