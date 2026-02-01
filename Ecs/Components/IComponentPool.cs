// IComponentPool.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;

namespace MonoKit.Ecs.Components;

public interface IComponentPool
{
    int Count { get; }
    bool Has(int entityId);
    void Remove(int entityId);
    int GetEntityAt(int index);
    IEnumerable<(int entityId, int denseIndex, int hash)> GetMappings();
}
