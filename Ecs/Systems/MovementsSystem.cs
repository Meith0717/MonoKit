// MovementsSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;

namespace MonoKit.Ecs.Systems;

public class MovementsSystem : ISystem
{
    public int Priority { get; } = 0;

    public void Update(double elapsedMs, World world)
    {
        var components = world.Components;
        var query = components.GetQuery().With<Transform2D>().With<Velocity2D>();

        query.ForEach(e =>
        {
            ref var transform = ref components.GetComponent<Transform2D>(e);
            ref var velocity = ref components.GetComponent<Velocity2D>(e);

            transform.Position += velocity.LinearVelocity * (float)elapsedMs;
            transform.Rotation += velocity.AngularVelocity * (float)elapsedMs;
        });
    }
}
