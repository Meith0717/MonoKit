// LifetimeSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.App.Input;
using MonoKit.GameRuntime.Ecs.Components;
using MonoKit.GameRuntime.Ecs.Entities;
using MonoKit.GameRuntime.Gameplay;

namespace MonoKit.GameRuntime.Ecs.Systems;

public class LifetimeSystem() : System<Lifetime>(-100)
{
    protected override void OnInitialize(World world) { }

    protected override void ProcessEntity(
        Entity e,
        ref Lifetime lifetime,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        if (lifetime.DestroyNow || lifetime.CoolDown <= 0)
        {
            world.DestroyEntity(e);
            return;
        }

        lifetime.CoolDown -= (float)elapsedMs;
    }
}
