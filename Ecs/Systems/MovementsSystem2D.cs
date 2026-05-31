using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.Ecs.Systems;

public class MovementsSystem2D : System<Transform2D, Velocity2D>
{
    public MovementsSystem2D() => Priority = 0;

    protected override void OnInitialize(World world) { }

    protected override void ProcessEntity(
        Entity e,
        ref Transform2D transform,
        ref Velocity2D velocity,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        transform.Position +=
            velocity.Velocity * velocity.NormalizedMovingDirection * (float)elapsedMs;
        transform.Rotation += velocity.AngularVelocity * (float)elapsedMs;
    }
}
