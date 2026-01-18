// IEntityComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace MonoKit.ECS;

public interface IEntityComponent
{
    void OnEntityDestroyed(Entity entity);
}
