// World.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.ArchetypeEcs;

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
    private readonly EntityManager _entityManager = new();
    private readonly EntityRecords _entityRecords = new();
    private readonly ArchetypeManager _archetypeManager = new();

    public int EntityCount => _entityManager.Count;
    public int ArchetypeCount => _archetypeManager.ArchetypeCount;

    public Entity CreateEntity()
    {
        var entity = _entityManager.Create();
        var row = _archetypeManager.EmptyArchetype.RowCount;
        _archetypeManager.EmptyArchetype.AddRow(entity, []);
        _entityRecords.SetEntityLocation(entity, _archetypeManager.EmptyArchetype, row);
        return entity;
    }

    public Entity CreateEntityWithComponents(params object[] components)
    {
        if (components == null || components.Length == 0)
            return CreateEntity();

        var componentTypes = new Type[components.Length];
        var seen = new HashSet<Type>(components.Length);
        for (var i = 0; i < components.Length; i++)
        {
            var t = components[i].GetType();
            if (!t.IsValueType)
                throw new ArgumentException($"Component {t.Name} must be a struct");
            if (!seen.Add(t))
                throw new ArgumentException($"Duplicate component type: {t.Name}");
            componentTypes[i] = t;
        }

        var archetypeType = new ArchetypeType(componentTypes);
        var archetype = _archetypeManager.GetOrCreateArchetype(archetypeType);

        var entity = _entityManager.Create();
        var row = archetype.RowCount;
        archetype.AddRow(entity, components);
        _entityRecords.SetEntityLocation(entity, archetype, row);
        return entity;
    }

    public Entity CreateEntity<T1>(T1 c1)
        where T1 : struct => CreateEntityWithComponents(c1);

    public Entity CreateEntity<T1, T2>(T1 c1, T2 c2)
        where T1 : struct
        where T2 : struct => CreateEntityWithComponents(c1, c2);

    public Entity CreateEntity<T1, T2, T3>(T1 c1, T2 c2, T3 c3)
        where T1 : struct
        where T2 : struct
        where T3 : struct => CreateEntityWithComponents(c1, c2, c3);

    public Entity CreateEntity<T1, T2, T3, T4>(T1 c1, T2 c2, T3 c3, T4 c4)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct => CreateEntityWithComponents(c1, c2, c3, c4);

    public bool DestroyEntity(Entity entity)
    {
        if (!_entityRecords.TryGetLocation(entity, out var archetype, out var row))
            return false;

        var lastEntity = archetype.RemoveRow(row);
        if (lastEntity.HasValue)
            _entityRecords.SetEntityLocation(lastEntity.Value, archetype, row);

        _entityRecords.Remove(entity);
        _entityManager.Destroy(entity);
        return true;
    }

    public bool IsAlive(Entity entity) => _entityManager.IsAlive(entity);

    public void AddComponent<T>(Entity entity, T component)
        where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var oldArchetype, out var oldRow))
            throw new ArgumentException("Entity does not exist");

        var oldArchetypeType = oldArchetype.Type;
        var componentType = typeof(T);

        if (oldArchetypeType.Contains(componentType))
        {
            oldArchetype.Get<T>(oldRow) = component;
            return;
        }

        // Create new archetype type with the component added
        var newComponentTypes = oldArchetypeType.ComponentTypes.Append(componentType).ToArray();
        var newArchetypeType = new ArchetypeType(newComponentTypes);
        var newArchetype = _archetypeManager.GetOrCreateArchetype(newArchetypeType);

        // Build column index mapping for the new archetype
        var typeToNewIndex = new Dictionary<Type, int>(newComponentTypes.Length);
        for (var i = 0; i < newComponentTypes.Length; i++)
            typeToNewIndex[newComponentTypes[i]] = i;

        // Copy existing components
        var newComponents = new object[newComponentTypes.Length];
        var oldComponents = oldArchetype.GetRowComponents(oldRow);
        for (var i = 0; i < oldComponents.Length; i++)
        {
            var oldCompType = oldArchetypeType.ComponentTypes[i];
            newComponents[typeToNewIndex[oldCompType]] = oldComponents[i];
        }

        // Add new component
        newComponents[typeToNewIndex[componentType]] = component!;

        // Move entity to new archetype
        var movedEntity = oldArchetype.RemoveRow(oldRow);
        if (movedEntity.HasValue)
            _entityRecords.SetEntityLocation(movedEntity.Value, oldArchetype, oldRow);

        var newRow = newArchetype.RowCount;
        newArchetype.AddRow(entity, newComponents);
        _entityRecords.SetEntityLocation(entity, newArchetype, newRow);
    }

    public void RemoveComponent<T>(Entity entity)
        where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var oldArchetype, out var oldRow))
            throw new ArgumentException("Entity does not exist or has been destroyed");

        var oldType = oldArchetype.Type;
        var componentType = typeof(T);

        if (!oldType.Contains(componentType))
            throw new InvalidOperationException($"Entity does not have component {typeof(T).Name}");

        // Create new archetype type without the component
        var newComponentTypes = oldType.ComponentTypes.Where(t => t != componentType).ToArray();
        Archetype newArchetype =
            newComponentTypes.Length == 0
                ? _archetypeManager.EmptyArchetype
                : _archetypeManager.GetOrCreateArchetype(new ArchetypeType(newComponentTypes));

        // Build column index mapping for the new archetype
        var oldToNewIndex = new Dictionary<Type, int>(newComponentTypes.Length);
        for (var i = 0; i < newComponentTypes.Length; i++)
            oldToNewIndex[newComponentTypes[i]] = i;

        // Copy existing components (excluding removed type)
        var newComponents = new object[newComponentTypes.Length];
        var oldComponents = oldArchetype.GetRowComponents(oldRow);
        for (var i = 0; i < oldComponents.Length; i++)
        {
            var oldCompType = oldType.ComponentTypes[i];
            if (
                oldCompType != componentType
                && oldToNewIndex.TryGetValue(oldCompType, out var newIdx)
            )
                newComponents[newIdx] = oldComponents[i];
        }

        // Move entity to new archetype
        var movedEntity = oldArchetype.RemoveRow(oldRow);
        if (movedEntity.HasValue)
            _entityRecords.SetEntityLocation(movedEntity.Value, oldArchetype, oldRow);

        var newRow = newArchetype.RowCount;
        newArchetype.AddRow(entity, newComponents);
        _entityRecords.SetEntityLocation(entity, newArchetype, newRow);
    }

    public ref T GetComponent<T>(Entity entity)
        where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var archetype, out var row))
            throw new ArgumentException("Entity does not exist or has been destroyed");
        return ref archetype.Get<T>(row);
    }

    public bool HasComponent<T>(Entity entity)
        where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var archetype, out _))
            return false;
        return archetype.Type.Contains(typeof(T));
    }

    // ========================================================================
    // CACHE-EFFICIENT ITERATION
    // ========================================================================

    public void ForEach<T1>(RefAction<T1> action)
        where T1 : struct
    {
        var componentType = typeof(T1);
        var archetypes = _archetypeManager.GetArchetypesWith(componentType);
        if (archetypes == null)
            return;

        foreach (var archetype in archetypes)
        {
            if (!archetype.TryGetColumn(componentType, out var column))
                continue;
            var columnArray = (T1[])column;
            for (var row = 0; row < archetype.RowCount; row++)
            {
                var entity = archetype.GetEntityAt(row);
                action(entity, ref columnArray[row]);
            }
        }
    }

    public void ForEach<T1, T2>(RefAction<T1, T2> action)
        where T1 : struct
        where T2 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var archetypesCollection = _archetypeManager.GetArchetypesWith(type1);
        if (archetypesCollection == null)
            return;

        foreach (var archetype in archetypesCollection)
        {
            if (!archetype.Type.Contains(type2))
                continue;

            if (
                !archetype.TryGetColumn(type1, out var column1)
                || !archetype.TryGetColumn(type2, out var column2)
            )
                continue;

            var col1Array = (T1[])column1;
            var col2Array = (T2[])column2;
            for (var row = 0; row < archetype.RowCount; row++)
            {
                var entity = archetype.GetEntityAt(row);
                action(entity, ref col1Array[row], ref col2Array[row]);
            }
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
        var archetypesCollection = _archetypeManager.GetArchetypesWith(type1);
        if (archetypesCollection == null)
            return;

        foreach (var archetype in archetypesCollection)
        {
            if (!archetype.Type.Contains(type2) || !archetype.Type.Contains(type3))
                continue;
            if (
                !archetype.TryGetColumn(type1, out var column1)
                || !archetype.TryGetColumn(type2, out var column2)
                || !archetype.TryGetColumn(type3, out var column3)
            )
                continue;
            var col1Array = (T1[])column1;
            var col2Array = (T2[])column2;
            var col3Array = (T3[])column3;
            for (var row = 0; row < archetype.RowCount; row++)
            {
                var entity = archetype.GetEntityAt(row);
                action(entity, ref col1Array[row], ref col2Array[row], ref col3Array[row]);
            }
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
        var archetypesCollection = _archetypeManager.GetArchetypesWith(type1);
        if (archetypesCollection == null)
            return;

        foreach (var archetype in archetypesCollection)
        {
            if (
                !archetype.Type.Contains(type2)
                || !archetype.Type.Contains(type3)
                || !archetype.Type.Contains(type4)
            )
                continue;
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
            for (var row = 0; row < archetype.RowCount; row++)
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

    // ========================================================================
    // DISPOSAL
    // ========================================================================

    public void Dispose()
    {
        _archetypeManager.Dispose();
        _entityRecords.Dispose();
        _entityManager.Dispose();
    }

    // ========================================================================
    // DEBUG
    // ========================================================================

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== AECS WORLD ===");
        sb.AppendLine($"Entities: {_entityManager.Count}");
        sb.AppendLine($"Archetypes: {_archetypeManager.ArchetypeCount}");
        foreach (var archetype in _archetypeManager.Archetypes)
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
}
