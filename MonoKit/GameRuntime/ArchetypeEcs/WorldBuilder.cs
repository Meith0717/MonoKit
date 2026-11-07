// MonoKit - WorldBuilder.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

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
