// SystemManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.ArchetypeEcs.Systems;

public class SystemManager(World world) : IDisposable
{
    private readonly List<ISystem> _systems = [];
    private readonly HashSet<ISystem> _addedSystems = [];

    public void Add(ISystem system)
    {
        system.Initialize(world);
        _systems.Add(system);
        _addedSystems.Add(system);
    }

    public void Update(
        double elapsedMs,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
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

        foreach (var system in displayList)
            sb.AppendLine($"[{system.GetType().Name}");

        return sb.ToString();
    }

    public void Dispose()
    {
        _systems.Clear();
        _addedSystems.Clear();
    }
}
