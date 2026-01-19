// EntitySystemManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;

namespace MonoKit.ECS;

public class EntitySystemManager
{
    private readonly List<IEntitySystem> _systems = [];
    private bool _isDirty = false;

    public void Add(IEntitySystem system)
    {
        _systems.Add(system);
        _isDirty = true;
    }

    public void Update(double elapsedMs, EntityComponentManager componentManager)
    {
        if (_isDirty)
        {
            _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            _isDirty = false;
        }

        foreach (var system in _systems)
            system.Update(elapsedMs, componentManager);
    }
}
