// ISpatialGrid3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace MonoKit.Spatial;

public interface ISpatialGrid3D
{
    void Add(Entity entity, Vector3 position, Vector3 size);
    void GetInRadius(Vector3 position, float radius, List<Entity> results);
    void GetInRadiusFast(Vector3 position, float radius, List<Entity> results);
    void Clear();
}
