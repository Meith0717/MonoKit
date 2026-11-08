// EntityProcessSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.Game.ArchetypeEcs.Entities;

namespace MonoKit.Game.ArchetypeEcs.Systems;

public interface IEntityProcessSystem : ISystem
{
    /// <summary>Notifies the system when an entity is destroyed.</summary>
    void OnEntityDestroyed(Entity entity);
}

public abstract class EntityProcessSystem<T1> : IUpdateSystem, IEntityProcessSystem
    where T1 : struct
{
    /// <summary>Hook: releases resources held by the derived system.</summary>
    public abstract void Dispose();

    /// <summary>Hook: sets the system up within the world.</summary>
    public abstract void Initialize(World world);

    /// <summary>Iterates matching archetypes and processes every entity.</summary>
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
                    Process(elapsedMs, world, archetype.GetEntityAt(row), ref columnArray[row]);
            }

            End();
        }
        finally
        {
            world.EndIteration();
        }
    }

    /// <summary>Hook: runs once before entities are processed.</summary>
    protected abstract void Begin();

    /// <summary>Hook: processes one entity's components this update.</summary>
    protected abstract void Process(double elapsedMs, World world, Entity entity, ref T1 c1);

    /// <summary>Hook: runs once after entities are processed.</summary>
    protected abstract void End();

    /// <summary>Hook: reacts when an entity is destroyed.</summary>
    public abstract void OnEntityDestroyed(Entity entity);
}

public abstract class EntityProcessSystem<T1, T2> : IUpdateSystem, IEntityProcessSystem
    where T1 : struct
    where T2 : struct
{
    /// <summary>Hook: releases resources held by the derived system.</summary>
    public abstract void Dispose();

    /// <summary>Hook: sets the system up within the world.</summary>
    public abstract void Initialize(World world);

    /// <summary>Iterates matching archetypes and processes every entity.</summary>
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
                    Process(
                        elapsedMs,
                        world,
                        archetype.GetEntityAt(row),
                        ref col1Array[row],
                        ref col2Array[row]
                    );
            }

            End();
        }
        finally
        {
            world.EndIteration();
        }
    }

    /// <summary>Hook: runs once before entities are processed.</summary>
    protected abstract void Begin();

    /// <summary>Hook: processes one entity's components this update.</summary>
    protected abstract void Process(
        double elapsedMs,
        World world,
        Entity entity,
        ref T1 c1,
        ref T2 c2
    );

    /// <summary>Hook: runs once after entities are processed.</summary>
    protected abstract void End();

    /// <summary>Hook: reacts when an entity is destroyed.</summary>
    public abstract void OnEntityDestroyed(Entity entity);
}

public abstract class EntityProcessSystem<T1, T2, T3> : IUpdateSystem, IEntityProcessSystem
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    /// <summary>Hook: releases resources held by the derived system.</summary>
    public abstract void Dispose();

    /// <summary>Hook: sets the system up within the world.</summary>
    public abstract void Initialize(World world);

    /// <summary>Iterates matching archetypes and processes every entity.</summary>
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
                        archetype.GetEntityAt(row),
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

    /// <summary>Hook: runs once before entities are processed.</summary>
    protected abstract void Begin();

    /// <summary>Hook: processes one entity's components this update.</summary>
    protected abstract void Process(
        double elapsedMs,
        World world,
        Entity entity,
        ref T1 c1,
        ref T2 c2,
        ref T3 c3
    );

    /// <summary>Hook: runs once after entities are processed.</summary>
    protected abstract void End();

    /// <summary>Hook: reacts when an entity is destroyed.</summary>
    public abstract void OnEntityDestroyed(Entity entity);
}

public abstract class EntityProcessSystem<T1, T2, T3, T4> : IUpdateSystem, IEntityProcessSystem
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    /// <summary>Hook: releases resources held by the derived system.</summary>
    public abstract void Dispose();

    /// <summary>Hook: sets the system up within the world.</summary>
    public abstract void Initialize(World world);

    /// <summary>Iterates matching archetypes and processes every entity.</summary>
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
                        archetype.GetEntityAt(row),
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

    /// <summary>Hook: runs once before entities are processed.</summary>
    protected abstract void Begin();

    /// <summary>Hook: processes one entity's components this update.</summary>
    protected abstract void Process(
        double elapsedMs,
        World world,
        Entity entity,
        ref T1 c1,
        ref T2 c2,
        ref T3 c3,
        ref T4 c4
    );

    /// <summary>Hook: runs once after entities are processed.</summary>
    protected abstract void End();

    /// <summary>Hook: reacts when an entity is destroyed.</summary>
    public abstract void OnEntityDestroyed(Entity entity);
}
