// SystemManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using MonoKit.Ecs.Components;

namespace MonoKit.Ecs.Systems;

public class SystemManager
{
    private readonly List<ISystem> _systems = [];
    private bool _isDirty = false;

    public void Add(ISystem system)
    {
        _systems.Add(system);
        _isDirty = true;
    }

    public void Update(double elapsedMs, ComponentManager componentManager)
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
