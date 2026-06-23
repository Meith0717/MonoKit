// SystemManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.Ecs.Systems;

public class SystemManager(World world)
{
    private readonly List<ISystem> _systems = [];
    private readonly HashSet<ISystem> _addedSystems = [];
    private bool _isDirty = false;

    public void Add(ISystem system)
    {
        system.Initialize(world);
        _systems.Add(system);
        _addedSystems.Add(system);
        _isDirty = true;
    }

    public void Remove(ISystem system)
    {
        _systems.Remove(system);
        _isDirty = true;
    }

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        if (_isDirty)
        {
            _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            _isDirty = false;
        }

        foreach (var system in _systems)
        {
            if (_addedSystems.Contains(system))
                system.Initialize(world);
            system.Update(elapsedMs, world, runtimeServices, inputHandler);
        }
        _addedSystems.Clear();
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
