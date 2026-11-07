// AppHost - LifetimeSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

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
