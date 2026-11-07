// MonoKit - IComponentPool.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;

namespace MonoKit.GameRuntime.Ecs.Components;

public interface IComponentPool
{
    int Count { get; }
    bool Has(int entityId);
    void Remove(int entityId);
    IEnumerable<(int entityId, int denseIndex, int hash)> GetMappings();
}
