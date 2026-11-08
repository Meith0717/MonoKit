// LifetimeSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;
using MonoKit.Game.Components;

namespace MonoKit.Game.Systems;

/// <summary>Counts down <see cref="Lifetime.CoolDown"/> and destroys the entity when it expires or DestroyNow is set.</summary>
public class LifetimeSystem : IUpdateSystem
{
    /// <summary>Hook: called once when the system joins the world.</summary>
    public void Initialize(World world) { }

    /// <summary>Counts down lifetimes and destroys expired entities.</summary>
    public void Update(double elapsedMs, World world)
    {
        world.ForEach<Lifetime>((Entity entity, ref Lifetime lifetime) =>
        {
            if (lifetime.DestroyNow || lifetime.CoolDown <= 0)
            {
                world.DestroyEntity(entity);
                return;
            }

            lifetime.CoolDown -= (float)elapsedMs;
        });
    }

    /// <summary>Releases system resources; none are held.</summary>
    public void Dispose() { }
}
