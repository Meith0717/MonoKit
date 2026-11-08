// AppHost - World.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Game.ArchetypeEcs.Archetypes;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.SpatialQueries;
using MonoKit.Game.ArchetypeEcs.Systems;

namespace MonoKit.Game.ArchetypeEcs;

public delegate void RefAction<T1>(Entity entity, ref T1 component1);

public delegate void RefAction<T1, T2>(Entity entity, ref T1 component1, ref T2 component2);

public delegate void RefAction<T1, T2, T3>(
    Entity entity,
    ref T1 component1,
    ref T2 component2,
    ref T3 component3
);

public delegate void RefAction<T1, T2, T3, T4>(
    Entity entity,
    ref T1 component1,
    ref T2 component2,
    ref T3 component3,
    ref T4 component4
);

public sealed class World : IDisposable
{
    internal readonly ArchetypeManager ArchetypeManager = new();

    private readonly ConcurrentQueue<Command> _commands = new();
    private readonly EntityManager _entityManager = new();
    private readonly EntityRecords _entityRecords = new();

    private readonly Dictionary<Type, object> _spatialContexts = new();

    private readonly List<ISystem> _systems = [];
    private int _iterationDepth;

    internal World()
    {
        ;
    }

    public int EntityCount => _entityManager.Count;
    public int ArchetypeCount => ArchetypeManager.ArchetypeCount;

    public void Dispose()
    {
        foreach (var system in _systems)
            system.Dispose();
        _systems.Clear();

        foreach (var context in _spatialContexts.Values)
            ((IDisposable)context).Dispose();
        _spatialContexts.Clear();

        ArchetypeManager.Dispose();
        _entityRecords.Dispose();
        _entityManager.Dispose();
        _commands.Clear();
        _iterationDepth = 0;
    }

    internal void AddSystem(ISystem system)
    {
        _systems.Add(system);
        system.Initialize(this);
    }

    public void Update(double elapsedMs)
    {
        foreach (var system in _systems)
        {
            if (system is not IUpdateSystem updateSystem)
                continue;
            BeginIteration();
            try
            {
                updateSystem.Update(elapsedMs, this);
            }
            finally
            {
                EndIteration();
            }
        }
    }

    public void Draw(double elapsedMs, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        foreach (var system in _systems)
        {
            if (system is not IDrawSystem drawSystem)
                continue;
            BeginIteration();
            try
            {
                drawSystem.Draw(elapsedMs, this, spriteBatch, graphicsDevice);
            }
            finally
            {
                EndIteration();
            }
        }
    }

    public void BeginIteration()
    {
        _iterationDepth++;
    }

    public void EndIteration()
    {
        if (_iterationDepth == 0)
            throw new InvalidOperationException("No iteration scope is active.");

        _iterationDepth--;
        if (_iterationDepth == 0)
            FlushCommands();
    }

    private void FlushCommands()
    {
        while (true)
        {
            var batch = new List<Command>();
            while (_commands.TryDequeue(out var command))
                batch.Add(command);
            if (batch.Count == 0)
                break;

            foreach (var command in batch)
                switch (command.Kind)
                {
                    case CommandKind.DestroyEntity:
                        DestroyEntityImmediate(command.Entity);
                        break;
                    case CommandKind.AddComponents:
                        AddComponentImmediate(command.Entity, (object[])command.Payload!);
                        break;
                    case CommandKind.RemoveComponent:
                        RemoveComponentImmediate(command.Entity, (Type)command.Payload!);
                        break;
                }
        }
    }

    public bool DestroyEntity(Entity entity)
    {
        if (!IsAlive(entity))
            return false;

        if (_iterationDepth > 0)
        {
            _commands.Enqueue(new Command(CommandKind.DestroyEntity, entity, null));
            return true;
        }

        return DestroyEntityImmediate(entity);
    }

    private bool DestroyEntityImmediate(Entity entity)
    {
        if (!_entityRecords.TryGetLocation(entity, out var archetype, out var row))
            return false;

        var movedEntity = archetype.RemoveRow(row);
        if (movedEntity.HasValue)
            _entityRecords.SetEntityLocation(movedEntity.Value, archetype, row);

        _entityRecords.Remove(entity);
        _entityManager.Destroy(entity);

        foreach (var system in _systems)
            if (system is IEntityProcessSystem processSystem)
                processSystem.OnEntityDestroyed(entity);

        return true;
    }

    public Entity CreateEntity()
    {
        var entity = _entityManager.Create();
        var row = ArchetypeManager.EmptyArchetype.RowCount;
        ArchetypeManager.EmptyArchetype.AddRow(entity, []);
        _entityRecords.SetEntityLocation(entity, ArchetypeManager.EmptyArchetype, row);
        return entity;
    }

    public Entity CreateEntityWithComponents(params object[] components)
    {
        if (components == null || components.Length == 0)
            return CreateEntity();

        ValidateComponents(components);

        var componentTypes = new Type[components.Length];
        for (var i = 0; i < components.Length; i++)
            componentTypes[i] = components[i].GetType();

        var archetypeType = new ArchetypeType(componentTypes);
        var archetype = ArchetypeManager.GetOrCreateArchetype(archetypeType);

        var entity = _entityManager.Create();
        var row = archetype.RowCount;
        archetype.AddRow(entity, components);
        _entityRecords.SetEntityLocation(entity, archetype, row);
        return entity;
    }

    public Entity CreateEntityWithComponents<T1>(T1 c1)
        where T1 : struct
    {
        var archetype = ArchetypeManager.GetOrCreateArchetype(new ArchetypeType(typeof(T1)));
        var entity = _entityManager.Create();
        var row = archetype.RowCount;
        archetype.AddRow(entity, c1);
        _entityRecords.SetEntityLocation(entity, archetype, row);
        return entity;
    }

    public Entity CreateEntityWithComponents<T1, T2>(T1 c1, T2 c2)
        where T1 : struct
        where T2 : struct
    {
        if (typeof(T1) == typeof(T2))
            throw new ArgumentException($"Duplicate component type: {typeof(T1).Name}");
        var archetype = ArchetypeManager.GetOrCreateArchetype(
            new ArchetypeType(typeof(T1), typeof(T2))
        );
        var entity = _entityManager.Create();
        var row = archetype.RowCount;
        archetype.AddRow(entity, c1, c2);
        _entityRecords.SetEntityLocation(entity, archetype, row);
        return entity;
    }

    public Entity CreateEntityWithComponents<T1, T2, T3>(T1 c1, T2 c2, T3 c3)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        if (typeof(T1) == typeof(T2) || typeof(T1) == typeof(T3) || typeof(T2) == typeof(T3))
            throw new ArgumentException("Duplicate component type");
        var archetype = ArchetypeManager.GetOrCreateArchetype(
            new ArchetypeType(typeof(T1), typeof(T2), typeof(T3))
        );
        var entity = _entityManager.Create();
        var row = archetype.RowCount;
        archetype.AddRow(entity, c1, c2, c3);
        _entityRecords.SetEntityLocation(entity, archetype, row);
        return entity;
    }

    public Entity CreateEntityWithComponents<T1, T2, T3, T4>(T1 c1, T2 c2, T3 c3, T4 c4)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        if (
            typeof(T1) == typeof(T2)
            || typeof(T1) == typeof(T3)
            || typeof(T1) == typeof(T4)
            || typeof(T2) == typeof(T3)
            || typeof(T2) == typeof(T4)
            || typeof(T3) == typeof(T4)
        )
            throw new ArgumentException("Duplicate component type");
        var archetype = ArchetypeManager.GetOrCreateArchetype(
            new ArchetypeType(typeof(T1), typeof(T2), typeof(T3), typeof(T4))
        );
        var entity = _entityManager.Create();
        var row = archetype.RowCount;
        archetype.AddRow(entity, c1, c2, c3, c4);
        _entityRecords.SetEntityLocation(entity, archetype, row);
        return entity;
    }

    public bool HasComponent<T>(Entity entity)
        where T : struct
    {
        return _entityRecords.TryGetLocation(entity, out var archetype, out _)
            && archetype.Contains(typeof(T));
    }

    public ref T GetComponent<T>(Entity entity)
        where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var archetype, out var row))
            throw new ArgumentException("Entity does not exist or has been destroyed");
        return ref archetype.Get<T>(row);
    }

    public bool IsAlive(Entity entity)
    {
        return _entityManager.IsAlive(entity);
    }

    public void AddComponent(Entity entity, params object[] components)
    {
        if (components == null || components.Length == 0)
            throw new ArgumentException("Components must not be null or empty");

        ValidateComponents(components);

        if (_iterationDepth > 0)
        {
            _commands.Enqueue(new Command(CommandKind.AddComponents, entity, components));
            return;
        }

        AddComponentImmediate(entity, components);
    }

    private static void ValidateComponents(object[] components)
    {
        var seen = new HashSet<Type>(components.Length);
        foreach (var component in components)
        {
            var t = component.GetType();
            if (!t.IsValueType)
                throw new ArgumentException($"Component {t.Name} must be a struct");
            if (!seen.Add(t))
                throw new ArgumentException($"Duplicate component type: {t.Name}");
        }
    }

    private void AddComponentImmediate(Entity entity, object[] components)
    {
        if (!_entityRecords.TryGetLocation(entity, out var oldArchetype, out var oldRow))
            throw new ArgumentException("Entity does not exist");

        foreach (var component in components)
            if (oldArchetype.Contains(component.GetType()))
                throw new ArgumentException(
                    $"Entity already has component {component.GetType().Name}"
                );

        var oldComponentTypes = oldArchetype.Type.ComponentTypes;
        var newComponentTypes = new Type[oldComponentTypes.Length + components.Length];
        Array.Copy(oldComponentTypes, newComponentTypes, oldComponentTypes.Length);
        for (var i = 0; i < components.Length; i++)
            newComponentTypes[oldComponentTypes.Length + i] = components[i].GetType();

        var newArchetype = ArchetypeManager.GetOrCreateArchetype(
            new ArchetypeType(newComponentTypes)
        );

        var typeToNewIndex = new Dictionary<Type, int>(newComponentTypes.Length);
        for (var i = 0; i < newComponentTypes.Length; i++)
            typeToNewIndex[newComponentTypes[i]] = i;

        var newComponents = new object[newComponentTypes.Length];
        var oldComponents = oldArchetype.GetRowComponents(oldRow);
        for (var i = 0; i < oldComponents.Length; i++)
            newComponents[typeToNewIndex[oldComponentTypes[i]]] = oldComponents[i];

        for (var i = 0; i < components.Length; i++)
            newComponents[typeToNewIndex[components[i].GetType()]] = components[i]!;

        MoveEntity(entity, oldArchetype, oldRow, newArchetype, newComponents);
    }

    public void RemoveComponent<T>(Entity entity)
        where T : struct
    {
        RemoveComponent(entity, typeof(T));
    }

    public void RemoveComponent(Entity entity, Type componentType)
    {
        if (!componentType.IsValueType)
            throw new ArgumentException($"Component {componentType.Name} must be a struct");

        if (_iterationDepth > 0)
        {
            _commands.Enqueue(new Command(CommandKind.RemoveComponent, entity, componentType));
            return;
        }

        RemoveComponentImmediate(entity, componentType);
    }

    private void RemoveComponentImmediate(Entity entity, Type componentType)
    {
        if (!_entityRecords.TryGetLocation(entity, out var oldArchetype, out var oldRow))
            throw new ArgumentException("Entity does not exist or has been destroyed");

        if (!oldArchetype.Contains(componentType))
            throw new InvalidOperationException(
                $"Entity does not have component {componentType.Name}"
            );

        var oldComponentTypes = oldArchetype.Type.ComponentTypes;
        var newComponentTypes = new Type[oldComponentTypes.Length - 1];
        var writeIndex = 0;
        for (var i = 0; i < oldComponentTypes.Length; i++)
            if (oldComponentTypes[i] != componentType)
                newComponentTypes[writeIndex++] = oldComponentTypes[i];

        var newArchetype =
            newComponentTypes.Length == 0
                ? ArchetypeManager.EmptyArchetype
                : ArchetypeManager.GetOrCreateArchetype(new ArchetypeType(newComponentTypes));

        var oldToNewIndex = new Dictionary<Type, int>(newComponentTypes.Length);
        for (var i = 0; i < newComponentTypes.Length; i++)
            oldToNewIndex[newComponentTypes[i]] = i;

        var newComponents = new object[newComponentTypes.Length];
        var oldComponents = oldArchetype.GetRowComponents(oldRow);
        for (var i = 0; i < oldComponents.Length; i++)
        {
            var oldCompType = oldComponentTypes[i];
            if (
                oldCompType != componentType
                && oldToNewIndex.TryGetValue(oldCompType, out var newIdx)
            )
                newComponents[newIdx] = oldComponents[i];
        }

        MoveEntity(entity, oldArchetype, oldRow, newArchetype, newComponents);
    }

    private void MoveEntity(
        Entity entity,
        Archetype oldArchetype,
        int oldRow,
        Archetype newArchetype,
        object[] newComponents
    )
    {
        var movedEntity = oldArchetype.RemoveRow(oldRow);
        if (movedEntity.HasValue)
            _entityRecords.SetEntityLocation(movedEntity.Value, oldArchetype, oldRow);

        var newRow = newArchetype.RowCount;
        newArchetype.AddRow(entity, newComponents);
        _entityRecords.SetEntityLocation(entity, newArchetype, newRow);
    }

    public void ForEach<T1>(RefAction<T1> action)
        where T1 : struct
    {
        var componentType = typeof(T1);
        var archetypes = ArchetypeManager.GetArchetypesWith(componentType);

        BeginIteration();
        try
        {
            foreach (var archetype in archetypes)
            {
                if (!archetype.TryGetColumn(componentType, out var column))
                    continue;
                var columnArray = (T1[])column;
                var rowCount = archetype.RowCount;
                for (var row = 0; row < rowCount; row++)
                {
                    var entity = archetype.GetEntityAt(row);
                    action(entity, ref columnArray[row]);
                }
            }
        }
        finally
        {
            EndIteration();
        }
    }

    public void ForEach<T1, T2>(RefAction<T1, T2> action)
        where T1 : struct
        where T2 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var archetypes = ArchetypeManager.GetArchetypesWith(type1);

        BeginIteration();
        try
        {
            foreach (var archetype in archetypes)
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
                {
                    var entity = archetype.GetEntityAt(row);
                    action(entity, ref col1Array[row], ref col2Array[row]);
                }
            }
        }
        finally
        {
            EndIteration();
        }
    }

    public void ForEach<T1, T2, T3>(RefAction<T1, T2, T3> action)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);
        var archetypes = ArchetypeManager.GetArchetypesWith(type1);

        BeginIteration();
        try
        {
            foreach (var archetype in archetypes)
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
                {
                    var entity = archetype.GetEntityAt(row);
                    action(entity, ref col1Array[row], ref col2Array[row], ref col3Array[row]);
                }
            }
        }
        finally
        {
            EndIteration();
        }
    }

    public void ForEach<T1, T2, T3, T4>(RefAction<T1, T2, T3, T4> action)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);
        var type4 = typeof(T4);
        var archetypes = ArchetypeManager.GetArchetypesWith(type1);

        BeginIteration();
        try
        {
            foreach (var archetype in archetypes)
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
                {
                    var entity = archetype.GetEntityAt(row);
                    action(
                        entity,
                        ref col1Array[row],
                        ref col2Array[row],
                        ref col3Array[row],
                        ref col4Array[row]
                    );
                }
            }
        }
        finally
        {
            EndIteration();
        }
    }

    public void ForEachParallel<T1>(RefAction<T1> action)
        where T1 : struct
    {
        var componentType = typeof(T1);
        var archetypes = ArchetypeManager.GetArchetypesWith(componentType);

        BeginIteration();
        try
        {
            foreach (var archetype in archetypes)
            {
                if (!archetype.TryGetColumn(componentType, out var column))
                    continue;
                var columnArray = (T1[])column;
                var rowCount = archetype.RowCount;
                Parallel.ForEach(
                    Partitioner.Create(0, rowCount),
                    range =>
                    {
                        for (var row = range.Item1; row < range.Item2; row++)
                            action(archetype.GetEntityAt(row), ref columnArray[row]);
                    }
                );
            }
        }
        finally
        {
            EndIteration();
        }
    }

    public void ForEachParallel<T1, T2>(RefAction<T1, T2> action)
        where T1 : struct
        where T2 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var archetypes = ArchetypeManager.GetArchetypesWith(type1);

        BeginIteration();
        try
        {
            foreach (var archetype in archetypes)
            {
                if (
                    !archetype.TryGetColumn(type1, out var column1)
                    || !archetype.TryGetColumn(type2, out var column2)
                )
                    continue;

                var col1Array = (T1[])column1;
                var col2Array = (T2[])column2;
                var rowCount = archetype.RowCount;
                Parallel.ForEach(
                    Partitioner.Create(0, rowCount),
                    range =>
                    {
                        for (var row = range.Item1; row < range.Item2; row++)
                            action(
                                archetype.GetEntityAt(row),
                                ref col1Array[row],
                                ref col2Array[row]
                            );
                    }
                );
            }
        }
        finally
        {
            EndIteration();
        }
    }

    public void ForEachParallel<T1, T2, T3>(RefAction<T1, T2, T3> action)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);
        var archetypes = ArchetypeManager.GetArchetypesWith(type1);

        BeginIteration();
        try
        {
            foreach (var archetype in archetypes)
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
                Parallel.ForEach(
                    Partitioner.Create(0, rowCount),
                    range =>
                    {
                        for (var row = range.Item1; row < range.Item2; row++)
                            action(
                                archetype.GetEntityAt(row),
                                ref col1Array[row],
                                ref col2Array[row],
                                ref col3Array[row]
                            );
                    }
                );
            }
        }
        finally
        {
            EndIteration();
        }
    }

    public void ForEachParallel<T1, T2, T3, T4>(RefAction<T1, T2, T3, T4> action)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);
        var type4 = typeof(T4);
        var archetypes = ArchetypeManager.GetArchetypesWith(type1);

        BeginIteration();
        try
        {
            foreach (var archetype in archetypes)
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
                Parallel.ForEach(
                    Partitioner.Create(0, rowCount),
                    range =>
                    {
                        for (var row = range.Item1; row < range.Item2; row++)
                            action(
                                archetype.GetEntityAt(row),
                                ref col1Array[row],
                                ref col2Array[row],
                                ref col3Array[row],
                                ref col4Array[row]
                            );
                    }
                );
            }
        }
        finally
        {
            EndIteration();
        }
    }

    public SpatialQueryContext<TPos> SpatialQueries<TPos>()
        where TPos : struct
    {
        if (_spatialContexts.TryGetValue(typeof(TPos), out var existing))
            return (SpatialQueryContext<TPos>)existing;

        var context = new SpatialQueryContext<TPos>();
        _spatialContexts[typeof(TPos)] = context;
        return context;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== AECS WORLD ===");
        sb.AppendLine($"Entities: {_entityManager.Count}");
        sb.AppendLine($"Archetypes: {ArchetypeManager.ArchetypeCount}");
        foreach (var archetype in ArchetypeManager.Archetypes)
        {
            var type = archetype.Type;
            sb.Append("\n  Archetype(");
            for (var i = 0; i < type.ComponentTypes.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(type.ComponentTypes[i].Name);
            }

            sb.Append($"): {archetype.RowCount} entities");
        }

        return sb.ToString();
    }

    private enum CommandKind
    {
        DestroyEntity,
        AddComponents,
        RemoveComponent,
    }

    private readonly struct Command(CommandKind kind, Entity entity, object? payload)
    {
        public readonly CommandKind Kind = kind;
        public readonly Entity Entity = entity;
        public readonly object? Payload = payload;
    }
}
