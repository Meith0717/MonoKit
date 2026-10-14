// SphBenchmark.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.
//
// Benchmarks for an SPH fluid simulation (poly6 / spiky / viscosity kernels)
// with 100k particles on top of the ArchetypeEcs spatial query pipeline.

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Microsoft.Xna.Framework;
using MonoKit.ArchetypeEcs;
using MonoKit.ArchetypeEcs.Entities;
using MonoKit.ArchetypeEcs.SpatialQueries;

namespace MonoKit.Benchmarks;

#region SPH model

public struct Particle { public Vector3 Position; }
public struct Velocity { public Vector3 Value; }
public struct Density { public float Value; }
public struct Pressure { public float Value; }
public struct Accel { public Vector3 Value; }

/// <summary>SPH constants and kernel coefficients (poly6, spiky, viscosity laplacian).</summary>
public static class Sph
{
    public const float H = 1.2f;             // smoothing radius
    public const float Spacing = 0.6f;       // initial particle spacing
    public const float Mass = 0.2137f;       // particle mass (rest density ~1.0)
    public const float Stiffness = 25f;      // pressure: P = k (rho - rho0)
    public const float Viscosity = 0.06f;
    public const float Dt = 1f / 120f;

    public static readonly Vector3 Gravity = new(0, -2.5f, 0);

    public const float H2 = H * H;

    // poly6: W(r) = 315/(64 pi h^9) (h^2 - r^2)^3
    public static readonly float Poly6 = 315f / (64f * MathF.PI * MathF.Pow(H, 9));

    // spiky gradient magnitude (excluding direction): 45/(pi h^6) (h - r)^2
    public static readonly float SpikyGrad = 45f / (MathF.PI * MathF.Pow(H, 6));

    // viscosity laplacian: 45/(pi h^6) (h - r)
    public static readonly float ViscLap = 45f / (MathF.PI * MathF.Pow(H, 6));
}

#endregion

/// <summary>
/// The SPH simulation on top of the ECS: spawns particles into a world,
/// builds the spatial frame each step, and provides the individual passes.
/// Positions are only mutated by <see cref="Step"/> and
/// <see cref="IntegratePass"/>; the pure passes keep the neighbor topology stable.
/// </summary>
public sealed class SphWorld : IDisposable
{
    public readonly MonoKit.ArchetypeEcs.World World = new WorldBuilder().Build();
    public readonly SpatialQueryContext<Particle> Spatial;
    public readonly int ParticleCount;
    public float RestDensity;

    private readonly List<int> _neighbors = new(4096);

    public SphWorld(int particleCount)
    {
        ParticleCount = particleCount;
        Spatial = World.SpatialQueries<Particle>();

        Spawn(particleCount);
        BuildFrame();
        DensityPass();
        RestDensity = MeasureAverageDensity();
    }

    private void Spawn(int count)
    {
        var rng = new Random(1337);
        var side = (int)MathF.Ceiling(MathF.Cbrt(count));
        var placed = 0;
        for (var y = 0; y < side && placed < count; y++)
        {
            for (var z = 0; z < side && placed < count; z++)
            {
                for (var x = 0; x < side && placed < count; x++)
                {
                    var jitterX = (float)(rng.NextDouble() - 0.5) * 0.05f;
                    var jitterY = (float)(rng.NextDouble() - 0.5) * 0.05f;
                    var jitterZ = (float)(rng.NextDouble() - 0.5) * 0.05f;
                    var position = new Vector3(
                        (x + 0.5f) * Sph.Spacing + jitterX,
                        y * Sph.Spacing + 0.1f + jitterY,
                        (z + 0.5f) * Sph.Spacing + jitterZ
                    );
                    World.CreateEntityWithComponents(
                        new Particle { Position = position },
                        new Velocity { Value = Vector3.Zero },
                        new Density(),
                        new Pressure(),
                        new Accel()
                    );
                    placed++;
                }
            }
        }
    }

    private float MeasureAverageDensity()
    {
        double sum = 0;
        var n = 0;
        World.ForEach<Density>((Entity entity, ref Density d) => { sum += d.Value; n++; });
        return n == 0 ? 1f : (float)(sum / n);
    }

    public void Dispose() => World.Dispose();

    // ========== passes ==========

    /// <summary>Rebuilds the spatial frame: gather, Morton sort, cell ranges.</summary>
    public void BuildFrame() => Spatial.BeginFrame(World, Sph.H, p => p.Position);

    /// <summary>
    /// Density pass through the raw field API with a hoisted neighbor buffer.
    /// This is the hot path a performance-focused SPH implementation should use.
    /// </summary>
    public void DensityPass()
    {
        var field = Spatial.Field;
        var positions = field.Positions;
        var density = Spatial.GetComponentCache<Density>();
        var count = field.Count;
        var neighbors = _neighbors;
        var h2 = Sph.H2;

        for (var i = 0; i < count; i++)
        {
            field.GetNeighbors(i, Sph.H, neighbors);
            var pi = positions[i].Position;
            float sum = 0f;
            foreach (var n in neighbors)
            {
                var r2 = Vector3.DistanceSquared(pi, positions[n].Position);
                if (r2 >= h2)
                    continue;
                var t = h2 - r2;
                sum += t * t * t;
            }
            density.GetRef(i).Value = sum * Sph.Mass * Sph.Poly6;
        }
    }

    /// <summary>
    /// Density pass through the per-entity neighbor API
    /// (World.ForEach + Spatial.ForEachNeighbor per entity).
    /// Measures the convenience path, including its per-call allocation.
    /// </summary>
    public void DensityPassPerEntityApi()
    {
        World.ForEach<Particle, Density>((Entity entity, ref Particle p, ref Density rho) =>
        {
            var pi = p.Position;
            float sum = 0f;
            Spatial.ForEachNeighbor<Particle>(entity, Sph.H, (Entity neighbor, ref Particle np) =>
            {
                var r2 = Vector3.DistanceSquared(pi, np.Position);
                if (r2 >= Sph.H2)
                    return;
                var t = Sph.H2 - r2;
                sum += t * t * t;
            });
            rho.Value = sum * Sph.Mass * Sph.Poly6;
        });
    }

    /// <summary>Pressure from density: P = k (rho - rho0).</summary>
    public void UpdatePressure()
    {
        var rest = RestDensity;
        World.ForEach<Density, Pressure>((Entity entity, ref Density d, ref Pressure pr) =>
            pr.Value = Sph.Stiffness * (d.Value - rest)
        );
    }

    /// <summary>
    /// Pressure (spiky) and viscosity (laplacian) forces over symmetric pairs
    /// (i &lt; j), writing both sides through the component caches.
    /// </summary>
    public void ForcesPass()
    {
        var field = Spatial.Field;
        var positions = field.Positions;
        var density = Spatial.GetComponentCache<Density>();
        var pressure = Spatial.GetComponentCache<Pressure>();
        var velocity = Spatial.GetComponentCache<Velocity>();
        var accel = Spatial.GetComponentCache<Accel>();
        var count = field.Count;
        var neighbors = _neighbors;
        var h2 = Sph.H2;

        for (var i = 0; i < count; i++)
            accel.GetRef(i).Value = Vector3.Zero;

        for (var i = 0; i < count; i++)
        {
            var pi = positions[i].Position;
            var vi = velocity.GetRef(i).Value;
            var rhoi = MathF.Max(density.GetRef(i).Value, 1e-6f);
            var pri = pressure.GetRef(i).Value;
            var acci = Vector3.Zero;

            field.GetNeighbors(i, Sph.H, neighbors);
            foreach (var n in neighbors)
            {
                if (n <= i)
                    continue; // symmetric pairs: each unordered pair once
                var d = positions[n].Position - pi;
                var r2 = d.LengthSquared();
                if (r2 >= h2)
                    continue;
                var r = MathF.Sqrt(r2);
                if (r < 1e-6f)
                    continue;

                var rhoj = MathF.Max(density.GetRef(n).Value, 1e-6f);
                var prj = pressure.GetRef(n).Value;

                // pressure force (spiky gradient), repulsive for P > 0
                var fCoef = Sph.Mass
                    * (pri + prj)
                    / (2f * rhoi * rhoj)
                    * Sph.SpikyGrad
                    * (Sph.H - r)
                    * (Sph.H - r)
                    / r;
                var f = d * fCoef;
                acci -= f;
                accel.GetRef(n).Value += f;

                // viscosity force (laplacian), momentum-conserving pair form
                var vCoef = Sph.Viscosity * Sph.Mass / rhoj * Sph.ViscLap * (Sph.H - r);
                var dv = velocity.GetRef(n).Value - vi;
                acci += dv * vCoef;
                accel.GetRef(n).Value -= dv * vCoef;
            }

            accel.GetRef(i).Value += acci;
        }
    }

    /// <summary>
    /// Semi-implicit Euler: v += (a + g) dt, then x += v dt with a damped ground
    /// plane at y = 0. Pass movePositions = false to keep positions unchanged.
    /// </summary>
    public void IntegratePass(bool movePositions)
    {
        World.ForEach<Velocity, Accel>((Entity entity, ref Velocity v, ref Accel a) =>
        {
            v.Value += (a.Value + Sph.Gravity) * Sph.Dt;
            a.Value = Vector3.Zero;
        });

        if (!movePositions)
            return;

        World.ForEach<Velocity, Particle>((Entity entity, ref Velocity v, ref Particle p) =>
        {
            p.Position += v.Value * Sph.Dt;
            if (p.Position.Y < 0f)
            {
                p.Position.Y = 0f;
                v.Value.Y *= -0.3f;
            }
        });
    }

    /// <summary>One full SPH step: frame, density, pressure, forces, integration.</summary>
    public void Step()
    {
        BuildFrame();
        DensityPass();
        UpdatePressure();
        ForcesPass();
        IntegratePass(true);
    }

    /// <summary>Runs a correctness smoke check and prints per-pass timings.</summary>
    public void RunSmoke(int steps)
    {
        Console.WriteLine($"Smoke: {ParticleCount} particles, rest density {RestDensity:F3}");

        var frame = Time("BuildFrame", () => BuildFrame());
        var density = Time("DensityPass", () => DensityPass());
        var pressure = Time("UpdatePressure", () => UpdatePressure());
        var forces = Time("ForcesPass", () => ForcesPass());
        var integrate = Time("IntegratePass", () => IntegratePass(true));
        Console.WriteLine(
            $"  first step: frame {frame:F1} ms, density {density:F1} ms, pressure {pressure:F1} ms, forces {forces:F1} ms, integrate {integrate:F1} ms"
        );

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < steps; i++)
            Step();
        sw.Stop();
        Console.WriteLine($"  {steps} full steps in {sw.Elapsed.TotalMilliseconds:F0} ms ({sw.Elapsed.TotalMilliseconds / steps:F1} ms/step)");

        var bad = 0;
        World.ForEach<Particle>((Entity entity, ref Particle p) =>
        {
            if (float.IsNaN(p.Position.X) || float.IsNaN(p.Position.Y) || float.IsNaN(p.Position.Z))
                bad++;
        });
        Console.WriteLine(bad == 0 ? "  positions finite: OK" : $"  FAIL: {bad} NaN positions");
        Environment.Exit(bad == 0 ? 0 : 1);
    }

    private static double Time(string name, Action action)
    {
        action(); // warmup (JIT + caches)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        action();
        return sw.Elapsed.TotalMilliseconds;
    }
}

#region benchmarks

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        // In-process toolchain avoids BenchmarkDotNet's generated projects
        // (the referenced library depends on MonoGame); ShortRun keeps the
        // suite fast for 100k-particle macro benchmarks.
        AddJob(Job.ShortRun.WithToolchain(InProcessEmitToolchain.Instance));
        AddDiagnoser(MemoryDiagnoser.Default);
        AddColumnProvider(DefaultColumnProviders.Instance);
        AddLogger(ConsoleLogger.Default);
        AddExporter(CsvExporter.Default, MarkdownExporter.Default);
    }
}

[Config(typeof(BenchmarkConfig))]
public class SphBenchmarks
{
    private SphWorld _sim = null!;

    [Params(100_000)]
    public int ParticleCount { get; set; }

    [GlobalSetup]
    public void Setup() => _sim = new SphWorld(ParticleCount);

    [GlobalCleanup]
    public void Cleanup() => _sim.Dispose();

    [Benchmark(Description = "SpatialFrame/Build (Morton sort + ranges)")]
    public void BuildSpatialFrame() => _sim.BuildFrame();

    [Benchmark(Description = "DensityPass/Field (hoisted buffer)")]
    public void DensityPassField() => _sim.DensityPass();

    [Benchmark(Description = "DensityPass/PerEntityApi")]
    public void DensityPassPerEntityApi() => _sim.DensityPassPerEntityApi();

    [Benchmark(Description = "PressureViscosityPass/SymmetricPairs")]
    public void ForcesPass() => _sim.ForcesPass();

    [Benchmark(Description = "IntegratePass/World.ForEach")]
    public void IntegratePass() => _sim.IntegratePass(true);

    [Benchmark(Description = "FullSphStep (frame+density+forces+integrate)")]
    public void FullSphStep() => _sim.Step();
}

#endregion

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--smoke")
        {
            var count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 100_000;
            using var sim = new SphWorld(count);
            sim.RunSmoke(steps: 240);
            return;
        }

        if (args.Length > 0 && args[0] == "--apis")
        {
            BenchmarkRunner.Run(
                new[]
                {
                    typeof(MonoKit.Benchmarks.EcsApi.EcsWorldApiBenchmarks),
                    typeof(MonoKit.Benchmarks.EcsApi.EcsSpatialApiBenchmarks),
                },
                new BenchmarkConfig(),
                args[1..]
            );
            return;
        }

        if (args.Length > 0 && args[0] == "--apismoke")
        {
            MonoKit.Benchmarks.EcsApi.EcsApiSmoke.Run();
            return;
        }

        BenchmarkRunner.Run(typeof(SphBenchmarks), new BenchmarkConfig(), args);
    }
}
