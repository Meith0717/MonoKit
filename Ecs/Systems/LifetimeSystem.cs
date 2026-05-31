using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.Ecs.Systems;

public class LifetimeSystem : System<Lifetime>
{
    public LifetimeSystem() => Priority = -100;

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
