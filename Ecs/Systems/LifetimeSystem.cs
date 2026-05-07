// LifetimeSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;
using MonoKit.Ecs.Querying;

namespace MonoKit.Ecs.Systems;

public class LifetimeSystem : ISystem
{
    public int Priority { get; } = -100;
    private EntityTypeTracker _tracker;
    private ComponentPool<Lifetime> _lifetimePool;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _lifetimePool = world.Components.GetOrCreatePool<Lifetime>();
    }

    public void Update(double elapsedMs, World world)
    {
        var entities = _tracker.GetEntitiesWith<Lifetime>();

        foreach (var e in entities)
        {
            ref var lifetime = ref _lifetimePool.Get(e.Id);

            if (lifetime.DestroyNow || lifetime.CoolDown <= 0)
            {
                world.DestroyEntity(e);
                continue;
            }

            lifetime.CoolDown -= (float)elapsedMs;
        }
    }
}
