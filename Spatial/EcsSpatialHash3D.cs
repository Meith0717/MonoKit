// EcsSpatialHash3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs.Entities;

namespace MonoKit.Spatial;

public class EcsSpatialHash3D(int cellSize)
{
    private readonly Dictionary<(int, int, int), List<Entity>> _grids = new();
    private readonly Dictionary<Entity, List<(int, int, int)>> _entityHashes = new();
    public readonly int CellSize = cellSize;

    public void UpdateEntity(Entity entity, Vector3 position, Vector3 size)
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

        var bottomFar = position + size;
        var start = Vector3.Floor(position / CellSize);
        var end = Vector3.Ceiling(bottomFar / CellSize);

        for (var x = (int)start.X; x < (int)end.X; x++)
        for (var y = (int)start.Y; y < (int)end.Y; y++)
        for (var z = (int)start.Z; z < (int)end.Z; z++)
        {
            var hash = (x, y, z);
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

    public void GetInRadius(Vector3 pos, float radius, List<Entity> results)
    {
        var startX = (int)float.Floor((pos.X - radius) / CellSize);
        var endX = (int)float.Ceiling((pos.X + radius) / CellSize);
        var startY = (int)float.Floor((pos.Y - radius) / CellSize);
        var endY = (int)float.Ceiling((pos.Y + radius) / CellSize);
        var startZ = (int)float.Floor((pos.Z - radius) / CellSize);
        var endZ = (int)float.Ceiling((pos.Z + radius) / CellSize);

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        for (var z = startZ; z < endZ; z++)
        {
            if (_grids.TryGetValue((x, y, z), out var cell))
                results.AddRange(cell);
        }
    }

    public void GetInBox(BoundingBox box, List<Entity> results)
    {
        var min = box.Min;
        var max = box.Max;

        var startX = (int)float.Floor(min.X / CellSize);
        var endX = (int)float.Ceiling(max.X / CellSize);
        var startY = (int)float.Floor(min.Y / CellSize);
        var endY = (int)float.Ceiling(max.Y / CellSize);
        var startZ = (int)float.Floor(min.Z / CellSize);
        var endZ = (int)float.Ceiling(max.Z / CellSize);

        for (var x = startX; x < endX; x++)
        for (var y = startY; y < endY; y++)
        for (var z = startZ; z < endZ; z++)
            if (_grids.TryGetValue((x, y, z), out var cell))
                results.AddRange(cell);
    }

    public void DrawDebug(
        GraphicsDevice graphics,
        Matrix view,
        Matrix projection,
        BasicEffect effect,
        Color color
    )
    {
        graphics.RasterizerState = RasterizerState.CullNone;
        graphics.DepthStencilState = DepthStencilState.Default;
        graphics.BlendState = BlendState.Opaque;

        var size = CellSize;

        var vertices = new VertexPositionColor[8]
        {
            new(new Vector3(0, 0, 0), color),
            new(new Vector3(size, 0, 0), color),
            new(new Vector3(size, size, 0), color),
            new(new Vector3(0, size, 0), color),
            new(new Vector3(0, 0, size), color),
            new(new Vector3(size, 0, size), color),
            new(new Vector3(size, size, size), color),
            new(new Vector3(0, size, size), color),
        };

        short[] indices =
        {
            0,
            1,
            1,
            2,
            2,
            3,
            3,
            0,
            4,
            5,
            5,
            6,
            6,
            7,
            7,
            4,
            0,
            4,
            1,
            5,
            2,
            6,
            3,
            7,
        };

        effect.View = view;
        effect.Projection = projection;
        effect.World = Matrix.Identity;
        effect.VertexColorEnabled = true;

        foreach (var ((x, y, z), _) in _grids)
        {
            effect.World = Matrix.CreateTranslation(new Vector3(x, y, z) * CellSize);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.DrawUserIndexedPrimitives(
                    PrimitiveType.LineList,
                    vertices,
                    0,
                    8,
                    indices,
                    0,
                    12
                );
            }
        }
    }
}
