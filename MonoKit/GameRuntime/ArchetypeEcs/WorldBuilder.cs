// WorldBuilder.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using MonoKit.GameRuntime.ArchetypeEcs.Systems;

namespace MonoKit.GameRuntime.ArchetypeEcs;

public class WorldBuilder
{
    private readonly List<ISystem> _systems = [];

    public WorldBuilder AddSystem(ISystem system)
    {
        _systems.Add(system);
        return this;
    }

    public World Build()
    {
        var world = new World();

        foreach (var system in _systems)
            world.AddSystem(system);

        return world;
    }
}
