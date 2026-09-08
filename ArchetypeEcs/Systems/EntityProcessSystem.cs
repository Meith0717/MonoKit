// EntitySystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.ArchetypeEcs.Systems;

public abstract class EntityProcessSystem : IUpdateSystem
{
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void Initialize(World world)
    {
        throw new System.NotImplementedException();
    }

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        world.ForEach();
    }

    public abstract void Begin();
    public abstract void Process(
        float elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    );
    public abstract void End();
    public abstract void OnEntityDestroyed(Entity entity);
}
