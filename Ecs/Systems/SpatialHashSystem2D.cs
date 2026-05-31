using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;
using MonoKit.Input;
using MonoKit.Spatial;

namespace MonoKit.Ecs.Systems;

public class SpatialHashSystem2D : System<Transform2D, Collider2D>, IOnEntityDestroyed
{
    private readonly EcsSpatialHash2D _grid;

    public SpatialHashSystem2D(EcsSpatialHash2D grid) => _grid = grid;

    public int Priority => 1;

    protected override void OnInitialize(World world) => _grid.Clear();

    protected override void ProcessEntity(
        Entity e,
        ref Transform2D transform,
        ref Collider2D collider,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        _grid.UpdateEntity(e, transform.Position, collider.Width, collider.Height);
    }

    public void OnEntityDestroyed(Entity entity) => _grid.RemoveEntity(entity);
}
