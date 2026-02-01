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
        var query = components.CreateFilter().With<TransformComponent>().With<VelocityComponent>();

        query.ForEach(e =>
        {
            if (!components.TryGetComponent<TransformComponent>(e, out var transform))
                return;
            if (!components.TryGetComponent<VelocityComponent>(e, out var velocity))
                return;

            transform.Position += velocity.Linear * (float)elapsedMs;
            transform.Rotation += velocity.Angular * (float)elapsedMs;

            components.AddComponent(e, transform);
        });
    }
}
