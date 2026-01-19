// EntitySystemManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;

namespace MonoKit.ECS;

public class EntitySystemManager
{
    private readonly List<IEntitySystem> _systems = [];

    public void Add(IEntitySystem system)
    {
        _systems.Add(system);
    }

    public void Update(double elapsedMs, EntityComponentManager componentManager)
    {
        foreach (var system in _systems)
            system.Update(elapsedMs, componentManager);
    }
}
