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
        var query = components.GetQuery().With<TransformComponent>().With<MovementComponent>();

        query.ForEach(e =>
        {
            if (!components.TryGetComponent<TransformComponent>(e, out var transform))
                return;
            if (!components.TryGetComponent<MovementComponent>(e, out var velocity))
                return;

            transform.Position += velocity.LinearVelocity * (float)elapsedMs;
            transform.Rotation += velocity.AngularVelocity * (float)elapsedMs;

            components.AddComponent(e, transform);
        });
    }
}
