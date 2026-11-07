// MonoKit - SystemManager.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Text;
using MonoKit.App.Input;
using MonoKit.GameRuntime.Ecs.Entities;
using MonoKit.GameRuntime.Gameplay;

namespace MonoKit.GameRuntime.Ecs.Systems;

public class SystemManager(World world) : IDisposable
{
    private readonly HashSet<ISystem> _addedSystems = [];
    private readonly List<ISystem> _systems = [];
    private bool _isDirty;

    public void Dispose()
    {
        foreach (var system in _systems)
            if (system is IDisposable disposableSystem)
                disposableSystem.Dispose();

        _systems.Clear();
        _addedSystems.Clear();
        _isDirty = false;
    }

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
            if (system is IOnEntityDestroyed listener)
                listener.OnEntityDestroyed(entity);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("--- System Manager (Execution Order) ---");
        var displayList = new List<ISystem>(_systems);
        displayList.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var system in displayList)
            sb.AppendLine($"[{system.Priority}] {system.GetType().Name}");

        return sb.ToString();
    }
}
