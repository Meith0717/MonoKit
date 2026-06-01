using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;
using MonoKit.Input;
using MonoKit.Spatial;

namespace MonoKit.Ecs.Systems;

public class SpatialHashSystem3D : System<Transform3D, Collider3D>
{
    private readonly ISpatialGrid3D _grid;

    public SpatialHashSystem3D(ISpatialGrid3D grid) => _grid = grid;

    public int Priority => 1;

    protected override void OnInitialize(World world) => _grid.Clear();

    protected override void OnUpdateStart(double elapsedMs, World world)
    {
        _grid.Clear();
    }

    protected override void ProcessEntity(
        Entity e,
        ref Transform3D transform,
        ref Collider3D collider,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        _grid.Add(e, transform.Position, collider.Size);
    }
}
