// ISpatialGrid3D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using MonoKit.Ecs.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MonoKit.Spatial;

public interface ISpatialGrid3D
{
    void Add(Entity entity, Vector3 position, Vector3 size);
    void GetInRadius(Vector3 position, float radius, List<Entity> results);
    void GetInRadiusFast(Vector3 position, float radius, List<Entity> results);
    void Clear();
}
