// MovementsSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Ecs.Components;

namespace MonoKit.Ecs.Systems;

public class MovementsSystem : ISystem
{
    public int Priority { get; } = 0;

    public void Update(double elapsedMs, ComponentManager components)
    {
        var query = components.CreateFilter().With<TransformComponent>().With<VelocityComponent>();

        foreach (var entity in query.Execute())
        {
            if (!components.TryGetComponent<TransformComponent>(entity, out var transform))
                continue;
            if (!components.TryGetComponent<VelocityComponent>(entity, out var velocity))
                continue;

            transform.Position += velocity.Linear * (float)elapsedMs;

            components.AddComponent(entity, transform);
        }
    }
}
