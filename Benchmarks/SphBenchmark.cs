// SphBenchmark.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

public static class Sph
{
    public const float H = 1.2f;
    public const float Spacing = 0.6f;
    public const float Mass = 0.2137f;
    public const float Stiffness = 25f;
    public const float Viscosity = 0.06f;
    public const float Dt = 1f / 120f;

    public static readonly Vector3 Gravity = new(0, -2.5f, 0);

    public const float H2 = H * H;

    public static readonly float Poly6 = 315f / (64f * MathF.PI * MathF.Pow(H, 9));

    public static readonly float SpikyGrad = 45f / (MathF.PI * MathF.Pow(H, 6));

    public static readonly float ViscLap = 45f / (MathF.PI * MathF.Pow(H, 6));
}

#endregion

public sealed class SphWorld : IDisposable
{
    public readonly MonoKit.ArchetypeEcs.World World = new WorldBuilder().Build();
    public readonly SpatialQueryContext<Particle> Spatial;
    public readonly int ParticleCount;
    public float RestDensity;

    private readonly List<int> _neighbors = new(4096);

    private Vector3[][] _parallelAccelBuffers = [];
    private List<int>[] _parallelNeighborLists = [];

    private void EnsureParallelBuffers(int count)
    {
        var threads = Environment.ProcessorCount;
        if (
            _parallelAccelBuffers.Length == threads
            && _parallelAccelBuffers[0].Length >= count
        )
            return;

        _parallelAccelBuffers = new Vector3[threads][];
        _parallelNeighborLists = new List<int>[threads];
        for (var t = 0; t < threads; t++)
        {
            _parallelAccelBuffers[t] = new Vector3[count];
            _parallelNeighborLists[t] = new List<int>(256);
        }
    }

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

    public void BuildFrame() => Spatial.BeginFrame(World, Sph.H, p => p.Position);

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

    public void UpdatePressure()
    {
        var rest = RestDensity;
        World.ForEach<Density, Pressure>((Entity entity, ref Density d, ref Pressure pr) =>
            pr.Value = Sph.Stiffness * (d.Value - rest)
        );
    }

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
                    continue;
                var d = positions[n].Position - pi;
                var r2 = d.LengthSquared();
                if (r2 >= h2)
                    continue;
                var r = MathF.Sqrt(r2);
                if (r < 1e-6f)
                    continue;

                var rhoj = MathF.Max(density.GetRef(n).Value, 1e-6f);
                var prj = pressure.GetRef(n).Value;

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

                var vCoef = Sph.Viscosity * Sph.Mass / rhoj * Sph.ViscLap * (Sph.H - r);
                var dv = velocity.GetRef(n).Value - vi;
                acci += dv * vCoef;
                accel.GetRef(n).Value -= dv * vCoef;
            }

            accel.GetRef(i).Value += acci;
        }
    }

    public void DensityPassParallel()
    {
        var field = Spatial.Field;
        var positions = field.Positions;
        var density = Spatial.GetComponentCache<Density>();
        var count = field.Count;
        var h2 = Sph.H2;

        EnsureParallelBuffers(count);
        var threads = _parallelAccelBuffers.Length;

        Parallel.For(
            0,
            threads,
            t =>
            {
                var neighbors = _parallelNeighborLists[t];
                var start = (int)((long)count * t / threads);
                var end = (int)((long)count * (t + 1) / threads);

                for (var i = start; i < end; i++)
                {
                    field.GetNeighbors(i, Sph.H, neighbors);
                    var pi = positions[i].Position;
                    float sum = 0f;
                    foreach (var n in neighbors)
                    {
                        var r2 = Vector3.DistanceSquared(pi, positions[n].Position);
                        if (r2 >= h2)
                            continue;
                        var tt = h2 - r2;
                        sum += tt * tt * tt;
                    }
                    density.GetRef(i).Value = sum * Sph.Mass * Sph.Poly6;
                }
            }
        );
    }

    public void ForcesPassParallel()
    {
        var field = Spatial.Field;
        var positions = field.Positions;
        var density = Spatial.GetComponentCache<Density>();
        var pressure = Spatial.GetComponentCache<Pressure>();
        var velocity = Spatial.GetComponentCache<Velocity>();
        var accel = Spatial.GetComponentCache<Accel>();
        var count = field.Count;
        var h2 = Sph.H2;

        EnsureParallelBuffers(count);
        var buffers = _parallelAccelBuffers;
        var threads = buffers.Length;

        Parallel.For(
            0,
            threads,
            t =>
            {
                var acc = buffers[t];
                Array.Clear(acc, 0, count);
                var neighbors = _parallelNeighborLists[t];
                var start = (int)((long)count * t / threads);
                var end = (int)((long)count * (t + 1) / threads);

                for (var i = start; i < end; i++)
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
                            continue;
                        var d = positions[n].Position - pi;
                        var r2 = d.LengthSquared();
                        if (r2 >= h2)
                            continue;
                        var r = MathF.Sqrt(r2);
                        if (r < 1e-6f)
                            continue;

                        var rhoj = MathF.Max(density.GetRef(n).Value, 1e-6f);
                        var prj = pressure.GetRef(n).Value;

                        var fCoef = Sph.Mass
                            * (pri + prj)
                            / (2f * rhoi * rhoj)
                            * Sph.SpikyGrad
                            * (Sph.H - r)
                            * (Sph.H - r)
                            / r;
                        var f = d * fCoef;
                        acci -= f;
                        acc[n] += f;

                        var vCoef = Sph.Viscosity * Sph.Mass / rhoj * Sph.ViscLap * (Sph.H - r);
                        var dv = velocity.GetRef(n).Value - vi;
                        acci += dv * vCoef;
                        acc[n] -= dv * vCoef;
                    }

                    acc[i] += acci;
                }
            }
        );

        for (var i = 0; i < count; i++)
        {
            var sum = Vector3.Zero;
            for (var t = 0; t < threads; t++)
                sum += buffers[t][i];
            accel.GetRef(i).Value = sum;
        }
    }

    public void StepParallel()
    {
        BuildFrame();
        DensityPassParallel();
        UpdatePressure();
        ForcesPassParallel();
        IntegratePass(true);
    }

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

    public void Step()
    {
        BuildFrame();
        DensityPass();
        UpdatePressure();
        ForcesPass();
        IntegratePass(true);
    }

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

        var densityParallel = Time("DensityPassParallel", () => DensityPassParallel());
        var forcesParallel = Time("ForcesPassParallel", () => ForcesPassParallel());
        Console.WriteLine(
            $"  parallel:  density {densityParallel:F1} ms ({density / densityParallel:F1}x), forces {forcesParallel:F1} ms ({forces / forcesParallel:F1}x)"
        );

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < steps; i++)
            Step();
        sw.Stop();
        Console.WriteLine($"  {steps} full steps in {sw.Elapsed.TotalMilliseconds:F0} ms ({sw.Elapsed.TotalMilliseconds / steps:F1} ms/step)");

        sw.Restart();
        for (var i = 0; i < steps; i++)
            StepParallel();
        sw.Stop();
        Console.WriteLine(
            $"  {steps} parallel steps in {sw.Elapsed.TotalMilliseconds:F0} ms ({sw.Elapsed.TotalMilliseconds / steps:F1} ms/step)"
        );

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
        action();
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

    [Benchmark(Description = "DensityPass/Parallel")]
    public void DensityPassParallel() => _sim.DensityPassParallel();

    [Benchmark(Description = "PressureViscosityPass/SymmetricPairsParallel")]
    public void ForcesPassParallel() => _sim.ForcesPassParallel();

    [Benchmark(Description = "IntegratePass/World.ForEach")]
    public void IntegratePass() => _sim.IntegratePass(true);

    [Benchmark(Description = "FullSphStep (frame+density+forces+integrate)")]
    public void FullSphStep() => _sim.Step();

    [Benchmark(Description = "FullSphStepParallel (frame+density+forces+integrate)")]
    public void FullSphStepParallel() => _sim.StepParallel();
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
