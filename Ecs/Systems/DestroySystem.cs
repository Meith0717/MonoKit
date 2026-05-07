// DestroySystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs.Components;

namespace MonoKit.Ecs.Systems;

public class DestroySystem : ISystem
{
    public int Priority { get; } = -100;
    private ComponentPool<Lifetime> _lifetimePool;

    public void Initialize(ComponentManager components)
    {
        _lifetimePool = components.GetOrCreatePool<Lifetime>();
    }

    public void Update(double elapsedMs, World world)
    {
        var components = world.Components;
        var query = components.GetQuery().With<Lifetime>();

        query.ForEach(e =>
        {
            ref var lifetime = ref _lifetimePool.Get(e.Id);

            if (lifetime.DestroyNow || lifetime.CoolDown <= 0)
            {
                world.DestroyEntity(e);
                return;
            }

            lifetime.CoolDown -= (float)elapsedMs;
        });
    }
}
