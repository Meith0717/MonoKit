// MonoKit - WorldBuilder.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using MonoKit.Game.ArchetypeEcs.Systems;

namespace MonoKit.Game.ArchetypeEcs;

/// <summary>Fluent builder that assembles a <see cref="World" /> with its systems.</summary>
public class WorldBuilder
{
    private readonly List<ISystem> _systems = [];

    /// <summary>Queues a system to be registered on <see cref="Build" />.</summary>
    public WorldBuilder AddSystem(ISystem system)
    {
        _systems.Add(system);
        return this;
    }

    /// <summary>Creates the world and initializes all queued systems.</summary>
    public World Build()
    {
        var world = new World();

        foreach (var system in _systems)
            world.AddSystem(system);

        return world;
    }
}
