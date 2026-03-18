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
        var query = components.GetQuery().With<DestroyComponent>();

        query.ForEach(e =>
        {
            if (!components.TryGetComponent<DestroyComponent>(e, out var destroy))
                return;

            if (destroy.DestroyNow || destroy.CoolDown <= 0)
            {
                world.DestroyEntity(e);
                return;
            }

            destroy.CoolDown -= (float)elapsedMs;
            components.AddComponent(e, destroy);
        });
    }
}
