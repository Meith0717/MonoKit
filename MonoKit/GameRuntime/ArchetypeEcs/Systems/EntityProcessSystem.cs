// EntityProcessSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.GameRuntime.ArchetypeEcs.Entities;

namespace MonoKit.GameRuntime.ArchetypeEcs.Systems;

public interface IEntityProcessSystem : ISystem
{
    void OnEntityDestroyed(Entity entity);
}

public abstract class EntityProcessSystem<T1> : IUpdateSystem, IEntityProcessSystem
    where T1 : struct
{
    public abstract void Dispose();

    public abstract void Initialize(World world);

    public void Update(double elapsedMs, World world)
    {
        world.BeginIteration();
        try
        {
            Begin();

            var componentType = typeof(T1);
            var archetypeCollection = world.ArchetypeManager.GetArchetypesWith(componentType);
            foreach (var archetype in archetypeCollection)
            {
                if (!archetype.TryGetColumn(componentType, out var column))
                    continue;
                var columnArray = (T1[])column;
                var rowCount = archetype.RowCount;
                for (var row = 0; row < rowCount; row++)
                    Process(elapsedMs, world, ref columnArray[row]);
            }

            End();
        }
        finally
        {
            world.EndIteration();
        }
    }

    protected abstract void Begin();

    protected abstract void Process(double elapsedMs, World world, ref T1 c1);

    protected abstract void End();

    public abstract void OnEntityDestroyed(Entity entity);
}

public abstract class EntityProcessSystem<T1, T2> : IUpdateSystem, IEntityProcessSystem
    where T1 : struct
    where T2 : struct
{
    public abstract void Dispose();

    public abstract void Initialize(World world);

    public void Update(double elapsedMs, World world)
    {
        world.BeginIteration();
        try
        {
            Begin();

            var type1 = typeof(T1);
            var type2 = typeof(T2);
            var archetypesCollection = world.ArchetypeManager.GetArchetypesWith(type1);
            foreach (var archetype in archetypesCollection)
            {
                if (
                    !archetype.TryGetColumn(type1, out var column1)
                    || !archetype.TryGetColumn(type2, out var column2)
                )
                    continue;

                var col1Array = (T1[])column1;
                var col2Array = (T2[])column2;
                var rowCount = archetype.RowCount;
                for (var row = 0; row < rowCount; row++)
                    Process(elapsedMs, world, ref col1Array[row], ref col2Array[row]);
            }

            End();
        }
        finally
        {
            world.EndIteration();
        }
    }

    protected abstract void Begin();

    protected abstract void Process(double elapsedMs, World world, ref T1 c1, ref T2 c2);

    protected abstract void End();

    public abstract void OnEntityDestroyed(Entity entity);
}

public abstract class EntityProcessSystem<T1, T2, T3> : IUpdateSystem, IEntityProcessSystem
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    public abstract void Dispose();

    public abstract void Initialize(World world);

    public void Update(double elapsedMs, World world)
    {
        world.BeginIteration();
        try
        {
            Begin();

            var type1 = typeof(T1);
            var type2 = typeof(T2);
            var type3 = typeof(T3);
            var archetypesCollection = world.ArchetypeManager.GetArchetypesWith(type1);
            foreach (var archetype in archetypesCollection)
            {
                if (
                    !archetype.TryGetColumn(type1, out var column1)
                    || !archetype.TryGetColumn(type2, out var column2)
                    || !archetype.TryGetColumn(type3, out var column3)
                )
                    continue;
                var col1Array = (T1[])column1;
                var col2Array = (T2[])column2;
                var col3Array = (T3[])column3;
                var rowCount = archetype.RowCount;
                for (var row = 0; row < rowCount; row++)
                    Process(
                        elapsedMs,
                        world,
                        ref col1Array[row],
                        ref col2Array[row],
                        ref col3Array[row]
                    );
            }

            End();
        }
        finally
        {
            world.EndIteration();
        }
    }

    protected abstract void Begin();

    protected abstract void Process(double elapsedMs, World world, ref T1 c1, ref T2 c2, ref T3 c3);

    protected abstract void End();

    public abstract void OnEntityDestroyed(Entity entity);
}

public abstract class EntityProcessSystem<T1, T2, T3, T4> : IUpdateSystem, IEntityProcessSystem
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    public abstract void Dispose();

    public abstract void Initialize(World world);

    public void Update(double elapsedMs, World world)
    {
        world.BeginIteration();
        try
        {
            Begin();

            var type1 = typeof(T1);
            var type2 = typeof(T2);
            var type3 = typeof(T3);
            var type4 = typeof(T4);
            var archetypesCollection = world.ArchetypeManager.GetArchetypesWith(type1);
            foreach (var archetype in archetypesCollection)
            {
                if (
                    !archetype.TryGetColumn(type1, out var column1)
                    || !archetype.TryGetColumn(type2, out var column2)
                    || !archetype.TryGetColumn(type3, out var column3)
                    || !archetype.TryGetColumn(type4, out var column4)
                )
                    continue;
                var col1Array = (T1[])column1;
                var col2Array = (T2[])column2;
                var col3Array = (T3[])column3;
                var col4Array = (T4[])column4;
                var rowCount = archetype.RowCount;
                for (var row = 0; row < rowCount; row++)
                    Process(
                        elapsedMs,
                        world,
                        ref col1Array[row],
                        ref col2Array[row],
                        ref col3Array[row],
                        ref col4Array[row]
                    );
            }

            End();
        }
        finally
        {
            world.EndIteration();
        }
    }

    protected abstract void Begin();

    protected abstract void Process(
        double elapsedMs,
        World world,
        ref T1 c1,
        ref T2 c2,
        ref T3 c3,
        ref T4 c4
    );

    protected abstract void End();

    public abstract void OnEntityDestroyed(Entity entity);
}
