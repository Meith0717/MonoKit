using System;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Querying;
using MonoKit.Gameplay;
using MonoKit.Input;

// ReSharper disable MemberCanBePrivate.Global

namespace MonoKit.Ecs.Systems;

public abstract class System<T1> : ISystem
    where T1 : struct
{
    protected ComponentPool<T1> Pool1 { get; private set; }
    private EntityTypeTracker _tracker;
    private Entity[] _buffer = new Entity[128];

    public int Priority { get; protected set; }

    public void Initialize(World world)
    {
        Pool1 = world.Components.GetOrCreatePool<T1>();
        _tracker = world.TypeTracker;
        OnInitialize(world);
    }

    protected abstract void OnInitialize(World world);

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        var span = _tracker.GetEntitiesWith<T1>(_buffer);

        if (span.Length == _buffer.Length)
        {
            Array.Resize(ref _buffer, _buffer.Length * 2);
            span = _tracker.GetEntitiesWith<T1>(_buffer);
        }

        for (int i = 0; i < span.Length; i++)
        {
            var e = span[i];
            ProcessEntity(e, ref Pool1.Get(e.Id), elapsedMs, world, runtimeServices, inputHandler);
        }
    }

    protected abstract void ProcessEntity(
        Entity entity,
        ref T1 c1,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    );
}

public abstract class System<T1, T2> : ISystem
    where T1 : struct
    where T2 : struct
{
    protected ComponentPool<T1> Pool1 { get; private set; }
    protected ComponentPool<T2> Pool2 { get; private set; }
    private EntityTypeTracker _tracker;
    private Entity[] _buffer = new Entity[128];

    public int Priority { get; protected set; }

    public void Initialize(World world)
    {
        Pool1 = world.Components.GetOrCreatePool<T1>();
        Pool2 = world.Components.GetOrCreatePool<T2>();
        _tracker = world.TypeTracker;
        OnInitialize(world);
    }

    protected abstract void OnInitialize(World world);

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        var span = _tracker.GetEntitiesWith<T1, T2>(_buffer);

        if (span.Length == _buffer.Length)
        {
            Array.Resize(ref _buffer, _buffer.Length * 2);
            span = _tracker.GetEntitiesWith<T1, T2>(_buffer);
        }

        for (int i = 0; i < span.Length; i++)
        {
            var e = span[i];
            ProcessEntity(
                e,
                ref Pool1.Get(e.Id),
                ref Pool2.Get(e.Id),
                elapsedMs,
                world,
                runtimeServices,
                inputHandler
            );
        }
    }

    protected abstract void ProcessEntity(
        Entity entity,
        ref T1 c1,
        ref T2 c2,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    );
}

public abstract class System<T1, T2, T3> : ISystem
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    protected ComponentPool<T1> Pool1 { get; private set; }
    protected ComponentPool<T2> Pool2 { get; private set; }
    protected ComponentPool<T3> Pool3 { get; private set; }
    private EntityTypeTracker _tracker;
    private Entity[] _buffer = new Entity[128];

    public int Priority { get; protected set; }

    public void Initialize(World world)
    {
        Pool1 = world.Components.GetOrCreatePool<T1>();
        Pool2 = world.Components.GetOrCreatePool<T2>();
        Pool3 = world.Components.GetOrCreatePool<T3>();
        _tracker = world.TypeTracker;
        OnInitialize(world);
    }

    protected abstract void OnInitialize(World world);

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        var span = _tracker.GetEntitiesWith<T1, T2, T3>(_buffer);

        if (span.Length == _buffer.Length)
        {
            Array.Resize(ref _buffer, _buffer.Length * 2);
            span = _tracker.GetEntitiesWith<T1, T2, T3>(_buffer);
        }

        for (int i = 0; i < span.Length; i++)
        {
            var e = span[i];
            ProcessEntity(
                e,
                ref Pool1.Get(e.Id),
                ref Pool2.Get(e.Id),
                ref Pool3.Get(e.Id),
                elapsedMs,
                world,
                runtimeServices,
                inputHandler
            );
        }
    }

    protected abstract void ProcessEntity(
        Entity entity,
        ref T1 c1,
        ref T2 c2,
        ref T3 c3,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    );
}

public abstract class System<T1, T2, T3, T4> : ISystem
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    protected ComponentPool<T1> Pool1 { get; private set; }
    protected ComponentPool<T2> Pool2 { get; private set; }
    protected ComponentPool<T3> Pool3 { get; private set; }
    protected ComponentPool<T4> Pool4 { get; private set; }
    private EntityTypeTracker _tracker;
    private Entity[] _buffer = new Entity[128];

    public int Priority { get; protected set; }

    public void Initialize(World world)
    {
        Pool1 = world.Components.GetOrCreatePool<T1>();
        Pool2 = world.Components.GetOrCreatePool<T2>();
        Pool3 = world.Components.GetOrCreatePool<T3>();
        Pool4 = world.Components.GetOrCreatePool<T4>();
        _tracker = world.TypeTracker;
        OnInitialize(world);
    }

    protected abstract void OnInitialize(World world);

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        var span = _tracker.GetEntitiesWith<T1, T2, T3, T4>(_buffer);

        if (span.Length == _buffer.Length)
        {
            Array.Resize(ref _buffer, _buffer.Length * 2);
            span = _tracker.GetEntitiesWith<T1, T2, T3, T4>(_buffer);
        }

        for (int i = 0; i < span.Length; i++)
        {
            var e = span[i];
            ProcessEntity(
                e,
                ref Pool1.Get(e.Id),
                ref Pool2.Get(e.Id),
                ref Pool3.Get(e.Id),
                ref Pool4.Get(e.Id),
                elapsedMs,
                world,
                runtimeServices,
                inputHandler
            );
        }
    }

    protected abstract void ProcessEntity(
        Entity entity,
        ref T1 c1,
        ref T2 c2,
        ref T3 c3,
        ref T4 c4,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    );
}
