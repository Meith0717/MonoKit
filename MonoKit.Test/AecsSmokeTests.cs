// AecsSmokeTests.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using MonoKit.GameRuntime.ArchetypeEcs;
using MonoKit.GameRuntime.ArchetypeEcs.Entities;

namespace MonoKit.Test;

public class AecsSmokeTests
{
    private struct Position
    {
        public Vector3 Value;
    }

    private struct Velocity
    {
        public Vector3 Value;
    }

    private struct Health
    {
        public int Value;
    }

    private struct Density
    {
        public float Value;
    }

    [Fact]
    public void SpawnForEachAndAccessors()
    {
        var world = new WorldBuilder().Build();
        const int n = 10_000;
        for (var i = 0; i < n; i++)
            world.CreateEntityWithComponents(
                new Position { Value = new Vector3(i, 0, 0) },
                new Velocity { Value = Vector3.One },
                new Health { Value = i },
                new Density { Value = 1f }
            );
        Assert.Equal(n, world.EntityCount);

        var sum = 0;
        world.ForEach<Position, Health>((Entity _, ref Position p, ref Health h) => sum += h.Value);
        Assert.Equal(n * (n - 1) / 2, sum);

        var e = world.CreateEntityWithComponents(
            new Position { Value = Vector3.Zero },
            new Health { Value = 42 }
        );
        Assert.True(world.HasComponent<Health>(e));
        Assert.Equal(42, world.GetComponent<Health>(e).Value);
        world.GetComponent<Health>(e).Value = 7;
        Assert.Equal(7, world.GetComponent<Health>(e).Value);
        world.Dispose();
    }

    [Fact]
    public void AddRemoveComponentMovesArchetypeAndPreservesData()
    {
        var world = new WorldBuilder().Build();
        var e = world.CreateEntityWithComponents(
            new Position { Value = new Vector3(1, 2, 3) },
            new Health { Value = 5 }
        );
        var archetypesBefore = world.ArchetypeCount;

        world.AddComponent(e, new Density { Value = 2f });
        Assert.True(world.HasComponent<Density>(e));
        Assert.Equal(new Vector3(1, 2, 3), world.GetComponent<Position>(e).Value);
        Assert.Equal(5, world.GetComponent<Health>(e).Value);
        Assert.Equal(archetypesBefore + 1, world.ArchetypeCount);

        world.RemoveComponent<Density>(e);
        Assert.False(world.HasComponent<Density>(e));
        Assert.Equal(5, world.GetComponent<Health>(e).Value);
        world.Dispose();
    }

    [Fact]
    public void DestroyInvalidatesHandleAndRecyclesId()
    {
        var world = new WorldBuilder().Build();
        var e = world.CreateEntityWithComponents(new Health { Value = 1 });
        Assert.True(world.IsAlive(e));
        Assert.True(world.DestroyEntity(e));
        Assert.False(world.IsAlive(e));

        var e2 = world.CreateEntity();
        Assert.Equal(e.Id, e2.Id);
        Assert.NotEqual(e.Generation, e2.Generation);
        Assert.False(world.IsAlive(e));
        world.Dispose();
    }

    [Fact]
    public void SpatialQueriesAreConsistent()
    {
        var world = new WorldBuilder().Build();
        const int side = 5;
        const float spacing = 0.6f;
        const float radius = 0.9f;
        for (var x = 0; x < side; x++)
        for (var y = 0; y < side; y++)
        for (var z = 0; z < side; z++)
            world.CreateEntityWithComponents(
                new Position { Value = new Vector3(x * spacing, y * spacing, z * spacing) },
                new Velocity { Value = Vector3.Zero }
            );

        var spatial = world.SpatialQueries<Position>();
        Assert.True(spatial.BeginFrame(world, radius, static p => p.Value));

        long selfPairs = 0;
        spatial.ForEachSelfNeighbor<Position, Position>(
            radius,
            (Entity a, ref Position pa, Entity b, ref Position pb) => selfPairs++
        );

        long perEntityPairs = 0;
        world.ForEach<Position>(
            (Entity e, ref Position p) =>
                spatial.ForEachNeighbor<Position>(
                    e,
                    radius,
                    (Entity n, ref Position np) => perEntityPairs++
                )
        );

        Assert.Equal(selfPairs, perEntityPairs);
        Assert.True(selfPairs > 0);

        // raw indexed pass: several SELF components via caches, no arity limit
        var velocities = spatial.GetComponentCache<Velocity>();
        long rawPairs = 0;
        spatial.ForEachSelfNeighborRaw(
            radius,
            (int i, Entity self, int j, Entity neighbor) =>
            {
                ref var v = ref velocities.GetRef(i);   // self velocity
                ref var p = ref spatial.GetComponentCache<Position>().GetRef(i); // self position
                v.Value += p.Value * 0f;
                rawPairs++;
            }
        );
        Assert.Equal(selfPairs, rawPairs);
        world.Dispose();
    }

    [Fact]
    public void ForEachParallelMatchesSerial()
    {
        var world = new WorldBuilder().Build();
        const int n = 50_000;
        for (var i = 0; i < n; i++)
            world.CreateEntityWithComponents(
                new Position { Value = new Vector3(i, 0, 0) },
                new Health { Value = i }
            );

        long serial = 0;
        world.ForEach<Position, Health>((Entity _, ref Position p, ref Health h) => serial += h.Value);

        long parallel = 0;
        world.ForEachParallel<Position, Health>((Entity _, ref Position p, ref Health h) =>
            System.Threading.Interlocked.Add(ref parallel, h.Value));

        Assert.Equal(serial, parallel);
        world.Dispose();
    }

    [Fact]
    public void ForEachParallelDeferredDestroyIsThreadSafe()
    {
        var world = new WorldBuilder().Build();
        const int n = 50_000;
        for (var i = 0; i < n; i++)
            world.CreateEntityWithComponents(new Health { Value = i });

        world.ForEachParallel<Health>((Entity e, ref Health h) => world.DestroyEntity(e));

        Assert.Equal(0, world.EntityCount);
        world.Dispose();
    }
}
