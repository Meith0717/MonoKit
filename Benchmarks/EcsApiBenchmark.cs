// EcsApiBenchmark.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Microsoft.Xna.Framework;
using MonoKit.ArchetypeEcs;
using MonoKit.ArchetypeEcs.Entities;
using MonoKit.ArchetypeEcs.SpatialQueries;
using MonoKit.ArchetypeEcs.Systems;

namespace MonoKit.Benchmarks.EcsApi;

public struct Position
{
    public Vector3 Value;
}

public struct Velocity
{
    public Vector3 Value;
}

public struct Health
{
    public int Value;
}

public struct Density
{
    public float Value;
}

public struct Extra
{
    public int Value;
}

public sealed class HealthTickSystem : EntityProcessSystem<Velocity, Health>
{
    public override void Dispose() { }

    public override void Initialize(World world) { }

    protected override void Begin() { }

    protected override void End() { }

    protected override void Process(double elapsedMs, World world, ref Velocity c1, ref Health c2) =>
        c2.Value++;

    public override void OnEntityDestroyed(Entity entity) { }
}

internal static class ApiWorld
{
    public const int TotalEntities = 100_000;
    public const int ExtraComponentEntities = 10_000;
    public const int BatchSize = 1_000;
    public const float Spacing = 0.6f;
    public const float QueryRadius = 0.9f;

    public static readonly Entity[] Entities = new Entity[TotalEntities];
    public static readonly Entity[] StructuralTargets = new Entity[BatchSize];

    public static World World = null!;
    public static World SystemWorld = null!;
    public static SpatialQueryContext<Position> Spatial = null!;

    public static void EnsureBuilt()
    {
        if (World != null)
            return;

        World = new WorldBuilder().Build();

        var side = (int)MathF.Ceiling(MathF.Cbrt(TotalEntities));
        var placed = 0;
        for (var y = 0; y < side && placed < TotalEntities; y++)
        for (var z = 0; z < side && placed < TotalEntities; z++)
        for (var x = 0; x < side && placed < TotalEntities; x++)
        {
            var position = new Position
            {
                Value = new Vector3((x + 0.5f) * Spacing, y * Spacing, (z + 0.5f) * Spacing),
            };
            var velocity = new Velocity { Value = new Vector3(x, y, z) };
            var health = new Health { Value = 1 };
            var density = new Density { Value = 1f };

            Entities[placed] =
                placed < TotalEntities - ExtraComponentEntities
                    ? World.CreateEntityWithComponents(position, velocity, health, density)
                    : World.CreateEntityWithComponents(
                        position,
                        velocity,
                        health,
                        density,
                        new Extra { Value = 1 }
                    );
            if (placed < BatchSize)
                StructuralTargets[placed] = Entities[placed];
            placed++;
        }

        Spatial = World.SpatialQueries<Position>();

        SystemWorld = new WorldBuilder().AddSystem(new HealthTickSystem()).Build();
        for (var i = 0; i < TotalEntities; i++)
            SystemWorld.CreateEntityWithComponents(
                new Velocity { Value = Vector3.Zero },
                new Health { Value = 0 }
            );
    }

    public static void DisposeAll()
    {
        World?.Dispose();
        SystemWorld?.Dispose();
        World = null!;
        SystemWorld = null!;
    }
}

[Config(typeof(BenchmarkConfig))]
public class EcsWorldApiBenchmarks
{
    private readonly Entity[] _batch = new Entity[ApiWorld.BatchSize];
    private long _sink;

    [GlobalSetup]
    public void Setup() => ApiWorld.EnsureBuilt();

    [Benchmark(Description = "CreateEntity+DestroyEntity, empty (x1000)")]
    public void CreateDestroyEmptyEntities()
    {
        for (var i = 0; i < ApiWorld.BatchSize; i++)
            _batch[i] = ApiWorld.World.CreateEntity();
        for (var i = 0; i < ApiWorld.BatchSize; i++)
            ApiWorld.World.DestroyEntity(_batch[i]);
    }

    [Benchmark(Description = "CreateEntityWithComponents(4)+DestroyEntity (x1000)")]
    public void CreateDestroyEntitiesWithComponents()
    {
        for (var i = 0; i < ApiWorld.BatchSize; i++)
        {
            _batch[i] = ApiWorld.World.CreateEntityWithComponents(
                new Position { Value = Vector3.Zero },
                new Velocity { Value = Vector3.Zero },
                new Health { Value = 1 },
                new Density { Value = 1f }
            );
        }

        for (var i = 0; i < ApiWorld.BatchSize; i++)
            ApiWorld.World.DestroyEntity(_batch[i]);
    }

    [Benchmark(Description = "AddComponent+RemoveComponent, archetype move (x1000)")]
    public void AddRemoveComponent()
    {
        for (var i = 0; i < ApiWorld.BatchSize; i++)
            ApiWorld.World.AddComponent(ApiWorld.StructuralTargets[i], new Extra { Value = 7 });
        for (var i = 0; i < ApiWorld.BatchSize; i++)
            ApiWorld.World.RemoveComponent<Extra>(ApiWorld.StructuralTargets[i]);
    }

    [Benchmark(Description = "Add+Remove deferred via command buffer (x1000)")]
    public void DeferredAddRemoveComponent()
    {
        var world = ApiWorld.World;
        world.BeginIteration();
        for (var i = 0; i < ApiWorld.BatchSize; i++)
            world.AddComponent(ApiWorld.StructuralTargets[i], new Extra { Value = 7 });
        for (var i = 0; i < ApiWorld.BatchSize; i++)
            world.RemoveComponent<Extra>(ApiWorld.StructuralTargets[i]);
        world.EndIteration();
    }

    [Benchmark(Description = "IsAlive (x100k)")]
    public void IsAlive100k()
    {
        var alive = 0;
        foreach (var entity in ApiWorld.Entities)
        {
            if (ApiWorld.World.IsAlive(entity))
                alive++;
        }
        _sink += alive;
    }

    [Benchmark(Description = "BeginIteration+EndIteration, empty scope (x100k)")]
    public void IterationScopes100k()
    {
        var world = ApiWorld.World;
        for (var i = 0; i < ApiWorld.TotalEntities; i++)
        {
            world.BeginIteration();
            world.EndIteration();
        }
    }

    [Benchmark(Description = "ForEach<T1> (100k rows, 2 archetypes)")]
    public void ForEach1() =>
        ApiWorld.World.ForEach<Position>((Entity entity, ref Position p) => _sink++);

    [Benchmark(Description = "ForEach<T1,T2> (100k rows, 2 archetypes)")]
    public void ForEach2() =>
        ApiWorld.World.ForEach<Position, Velocity>(
            (Entity entity, ref Position p, ref Velocity v) => _sink++
        );

    [Benchmark(Description = "ForEach<T1,T2,T3> (100k rows, 2 archetypes)")]
    public void ForEach3() =>
        ApiWorld.World.ForEach<Position, Velocity, Health>(
            (Entity entity, ref Position p, ref Velocity v, ref Health h) => _sink++
        );

    [Benchmark(Description = "ForEach<T1,T2,T3,T4> (100k rows, 2 archetypes)")]
    public void ForEach4() =>
        ApiWorld.World.ForEach<Position, Velocity, Health, Density>(
            (Entity entity, ref Position p, ref Velocity v, ref Health h, ref Density d) =>
                _sink++
        );

    [Benchmark(Description = "World.Update, EntityProcessSystem<T1,T2> (100k rows)")]
    public void WorldUpdateWithSystem() => ApiWorld.SystemWorld.Update(1.0);

    [Benchmark(Description = "World.Draw, no draw systems (x100k dispatch loop)")]
    public void WorldDrawNoDrawSystems()
    {
        for (var i = 0; i < ApiWorld.TotalEntities; i++)
            ApiWorld.SystemWorld.Draw(1.0);
    }

    [Benchmark(Description = "EntityCount+ArchetypeCount getters (x100k)")]
    public void WorldGetters100k()
    {
        for (var i = 0; i < ApiWorld.TotalEntities; i++)
            _sink += ApiWorld.World.EntityCount + ApiWorld.World.ArchetypeCount;
    }

    [Benchmark(Description = "World.ToString (2-archetype diagnostics dump)")]
    public void WorldToString() => _sink += ApiWorld.World.ToString().Length;
}

[Config(typeof(BenchmarkConfig))]
public class EcsSpatialApiBenchmarks
{
    private readonly List<int> _neighbors = new(4096);
    private long _sink;
    private int _pairs;
    private SpatialQueryContext<Position> _scratch = null!;

    [GlobalSetup]
    public void Setup()
    {
        ApiWorld.EnsureBuilt();
        RefreshFrame();
    }

    [IterationSetup]
    public void RefreshFrame() =>
        ApiWorld.Spatial.BeginFrame(ApiWorld.World, ApiWorld.QueryRadius, static p => p.Value);

    [Benchmark(Description = "SpatialQueries<TPos>() context lookup (x100k)")]
    public void SpatialContextLookup100k()
    {
        for (var i = 0; i < ApiWorld.TotalEntities; i++)
            _scratch = ApiWorld.World.SpatialQueries<Position>();
    }

    [Benchmark(Description = "BeginFrame: gather+Morton sort+ranges (100k)")]
    public void SpatialBeginFrame() =>
        ApiWorld.Spatial.BeginFrame(ApiWorld.World, ApiWorld.QueryRadius, static p => p.Value);

    [Benchmark(Description = "SpatialField.GetNeighbors (100k queries, r=0.9)")]
    public void SpatialGetNeighbors100k()
    {
        var field = ApiWorld.Spatial.Field;
        var count = field.Count;
        var found = 0;
        for (var i = 0; i < count; i++)
        {
            field.GetNeighbors(i, ApiWorld.QueryRadius, _neighbors);
            found += _neighbors.Count;
        }
        _sink += found;
    }

    [Benchmark(Description = "ComponentCache.Has+TryGet (x100k)")]
    public void ComponentCacheHasTryGet100k()
    {
        var field = ApiWorld.Spatial.Field;
        var cache = ApiWorld.Spatial.GetComponentCache<Health>();
        var count = field.Count;
        var found = 0;
        for (var i = 0; i < count; i++)
        {
            if (!cache.Has(i))
                continue;
            if (cache.TryGet(i, out var health))
                found += health.Value;
        }
        _sink += found;
    }

    [Benchmark(Description = "ComponentCache.GetRef write (x100k)")]
    public void ComponentCacheGetRefWrite100k()
    {
        var field = ApiWorld.Spatial.Field;
        var cache = ApiWorld.Spatial.GetComponentCache<Health>();
        for (var i = 0; i < field.Count; i++)
            cache.GetRef(i).Value++;
    }

    [Benchmark(Description = "ForEachNeighbor all-pairs (100k entities, r=0.9)")]
    public void ForEachNeighborAllPairs()
    {
        _pairs = 0;
        ApiWorld.Spatial.ForEachNeighbor<Position>(
            ApiWorld.QueryRadius,
            (Entity entity, Entity neighbor, ref Position p) => _pairs++
        );
        _sink += _pairs;
    }

    [Benchmark(Description = "ForEachNeighborParallel all-pairs (100k entities, r=0.9)")]
    public void ForEachNeighborParallelAllPairs()
    {
        ApiWorld.Spatial.ForEachNeighborParallel<Position>(
            ApiWorld.QueryRadius,
            (Entity entity, Entity neighbor, ref Position p) =>
            {
                if (p.Value.X > 1e30f)
                    _pairs++;
            }
        );
        _sink += _pairs;
    }

    [Benchmark(Description = "ForEachNeighbor per-entity API (100k calls, r=0.9)")]
    public void ForEachNeighborPerEntity()
    {
        _pairs = 0;
        ApiWorld.World.ForEach<Position>(
            (Entity entity, ref Position p) =>
                ApiWorld.Spatial.ForEachNeighbor<Position>(
                    entity,
                    ApiWorld.QueryRadius,
                    (Entity neighbor, ref Position np) => _pairs++
                )
        );
        _sink += _pairs;
    }

    [Benchmark(Description = "ForEachNeighbor nested, level 2 (100k + 1.3M queries, r=0.9)")]
    public void ForEachNeighborNestedLevel2()
    {
        _pairs = 0;
        ApiWorld.World.ForEach<Position>(
            (Entity entity, ref Position p) =>
                ApiWorld.Spatial.ForEachNeighbor<Position>(
                    entity,
                    ApiWorld.QueryRadius,
                    (Entity neighbor, ref Position np) =>
                    {
                        _pairs++;
                        ApiWorld.Spatial.ForEachNeighbor<Position>(
                            neighbor,
                            ApiWorld.QueryRadius,
                            (Entity n2, ref Position p2) => _pairs++
                        );
                    }
                )
        );
        _sink += _pairs;
    }
}

public static class EcsApiSmoke
{
    public static void Run()
    {
        Console.WriteLine("ECS API smoke: 100k entities, 1 warmup + 1 timed run per benchmark");

        var types = new[] { typeof(EcsWorldApiBenchmarks), typeof(EcsSpatialApiBenchmarks) };
        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type)!;
            type.GetMethod("Setup")!.Invoke(instance, null);
            var refresh = type.GetMethod("RefreshFrame");

            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttribute<BenchmarkAttribute>() != null)
                .ToArray();

            Console.WriteLine($"\n{type.Name}");
            foreach (var method in methods)
            {
                refresh?.Invoke(instance, null);
                method.Invoke(instance, null);
                refresh?.Invoke(instance, null);

                var sw = Stopwatch.StartNew();
                method.Invoke(instance, null);
                sw.Stop();

                var description = method.GetCustomAttribute<BenchmarkAttribute>()!.Description;
                Console.WriteLine($"  {sw.Elapsed.TotalMilliseconds,9:F2} ms  {description ?? method.Name}");
            }
        }

        var worldCount = ApiWorld.World.EntityCount;
        var systemWorldCount = ApiWorld.SystemWorld.EntityCount;
        var ok =
            worldCount == ApiWorld.TotalEntities && systemWorldCount == ApiWorld.TotalEntities;
        Console.WriteLine(
            ok
                ? $"\nWorld state restored: {worldCount} + {systemWorldCount} entities OK"
                : $"\nFAIL: world state drifted ({worldCount}, {systemWorldCount})"
        );
        ApiWorld.DisposeAll();
        Environment.Exit(ok ? 0 : 1);
    }
}
