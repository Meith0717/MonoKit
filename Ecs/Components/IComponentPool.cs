// IComponentPool.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace MonoKit.Ecs.Components;

public interface IComponentPool
{
    int Count { get; }
    bool Has(int entityId);
    int GetEntityAt(int index);
}
