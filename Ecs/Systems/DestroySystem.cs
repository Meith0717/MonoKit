// DestroySystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;

namespace MonoKit.Ecs.Systems;

public class DestroySystem : ISystem
{
    public int Priority { get; } = -100;

    public void Update(double elapsedMs, World world)
    {
        var components = world.Components;
        var query = components.GetQuery().With<Lifetime>();

        query.ForEach(e =>
        {
            ref var lifetime = ref components.GetComponent<Lifetime>(e);

            if (lifetime.DestroyNow || lifetime.CoolDown <= 0)
            {
                world.DestroyEntity(e);
                return;
            }

            lifetime.CoolDown -= (float)elapsedMs;
        });
    }
}
