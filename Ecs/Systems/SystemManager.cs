// SystemManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using MonoKit.Ecs.Entities;

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

    public void Update(double elapsedMs, World world)
    {
        if (_isDirty)
        {
            _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            _isDirty = false;
        }

        foreach (var system in _systems)
            system.Update(elapsedMs, world);
    }

    public void NotifyEntityDestroyed(Entity entity)
    {
        foreach (var system in _systems)
        {
            if (system is IOnEntityDestroyed listener)
                listener.OnEntityDestroyed(entity);
        }
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("--- System Manager (Execution Order) ---");
        var displayList = new List<ISystem>(_systems);
        displayList.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var system in displayList)
            sb.AppendLine($"[{system.Priority}] {system.GetType().Name}");

        return sb.ToString();
    }
}
