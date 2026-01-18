// IEntityComponentSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.ObjectModel;
using MonoKit.Gameplay;

namespace MonoKit.ECS;

public interface IEntityComponentSystem
{
    void Update(
        double elapsedMs,
        RuntimeContainer runtimeContainer,
        ReadOnlyCollection<Entity> entities
    );
}
