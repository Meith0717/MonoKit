// AecsSmokeTests.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.Spatial;

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
    public void ComponentColumnSurvivesArchetypeGrowth()
    {
        var world = new WorldBuilder().Build();
        var column = world.GetComponentColumn<Position>();

        var entities = new List<Entity>();
        for (var i = 0; i < 100; i++)
        {
            var e = world.CreateEntityWithComponents(new Position { Value = new Vector3(i, 0, 0) });
            entities.Add(e);
            ref var p = ref column.GetRef(e);
            Assert.Equal(new Vector3(i, 0, 0), p.Value);
            p.Value += new Vector3(1, 0, 0);
        }

        // Growth replaced the archetype's column arrays; the column must follow.
        foreach (var e in entities)
        {
            ref var p = ref column.GetRef(e);
            Assert.Equal(e.Id + 1, p.Value.X);
        }

        Assert.True(column.Has(entities[0]));
        world.Dispose();
    }

    [Fact]
    public void SpatialHashMatchesBruteForce()
    {
        var world = new WorldBuilder().Build();
        var rand = new Random(7);
        const int n = 500;
        const float radius = 2.5f;
        var entities = new List<Entity>();
        for (var i = 0; i < n; i++)
            entities.Add(
                world.CreateEntityWithComponents(
                    new Position
                    {
                        Value = new Vector3(
                            rand.Next(-20, 20),
                            rand.Next(-20, 20),
                            rand.Next(-20, 20)
                        ),
                    },
                    new Velocity { Value = Vector3.Zero }
                )
            );

        var hash = new SpatialHash(radius);
        hash.Rebuild<Position>(world, static p => p.Value);

        var positions = world.GetComponentColumn<Position>();
        var neighbors = new List<Entity>();
        foreach (var e in entities)
        {
            var center = positions.GetRef(e).Value;
            hash.Query(center, radius, neighbors);

            var brute = new HashSet<Entity>();
            foreach (var other in entities)
            {
                var d = positions.GetRef(other).Value - center;
                if (d.LengthSquared() < radius * radius)
                    brute.Add(other);
            }

            Assert.Equal(brute.Count, neighbors.Count);
            Assert.True(brute.SetEquals(neighbors));
        }

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
        world.ForEach<Position, Health>(
            (Entity _, ref Position p, ref Health h) => serial += h.Value
        );

        long parallel = 0;
        world.ForEachParallel<Position, Health>(
            (Entity _, ref Position p, ref Health h) =>
                System.Threading.Interlocked.Add(ref parallel, h.Value)
        );

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
