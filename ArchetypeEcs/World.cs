// World.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Linq;
using System.Collections.Generic;

namespace MonoKit.ArchetypeEcs;

// Custom delegates for ref parameters
public delegate void RefAction<T1>(Entity entity, ref T1 component1);
public delegate void RefAction<T1, T2>(Entity entity, ref T1 component1, ref T2 component2);
public delegate void RefAction<T1, T2, T3>(Entity entity, ref T1 component1, ref T2 component2, ref T3 component3);
public delegate void RefAction<T1, T2, T3, T4>(Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4);

/// <summary>
/// The main ECS world container.
/// Uses a clean architecture with separate managers:
/// - EntityRecords: Tracks entity locations (Archetype, Row)
/// - ArchetypeManager: Manages archetype creation and component indexing
/// - ComponentRecord: Maps (ComponentType, ArchetypeType) to ColumnIndex
/// - EntityManager: Manages entity ID generation
/// </summary>
public sealed class World : IDisposable
{
    private readonly EntityManager _entityManager;
    private readonly EntityRecords _entityRecords;
    private readonly ArchetypeManager _archetypeManager;
    private readonly ComponentRecord _componentRecord;

    public int EntityCount => _entityManager.Count;
    public int ArchetypeCount => _archetypeManager.ArchetypeCount;

    public World()
    {
        _entityManager = new EntityManager();
        _componentRecord = new ComponentRecord();
        _archetypeManager = new ArchetypeManager(_componentRecord);
        _entityRecords = new EntityRecords();
    }

    public Entity CreateEntity()
    {
        var entity = _entityManager.Create();
        var row = _archetypeManager.EmptyArchetype.RowCount;
        _archetypeManager.EmptyArchetype.AddRow(entity, []);
        _entityRecords.SetLocation(entity, _archetypeManager.EmptyArchetype, row);
        return entity;
    }

    public Entity CreateEntity<T1>(T1 component1) where T1 : struct
    {
        var entity = CreateEntity();
        AddComponent(entity, component1);
        return entity;
    }

    public Entity CreateEntity<T1, T2>(T1 component1, T2 component2)
        where T1 : struct
        where T2 : struct
    {
        var entity = CreateEntity();
        AddComponent(entity, component1);
        AddComponent(entity, component2);
        return entity;
    }

    public Entity CreateEntity<T1, T2, T3>(T1 component1, T2 component2, T3 component3)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var entity = CreateEntity();
        AddComponent(entity, component1);
        AddComponent(entity, component2);
        AddComponent(entity, component3);
        return entity;
    }

    public bool DestroyEntity(Entity entity)
    {
        if (!_entityRecords.TryGetLocation(entity, out var archetype, out var row))
            return false;

        var movedEntity = archetype.RemoveRow(row);
        if (movedEntity.HasValue)
            _entityRecords.SetLocation(movedEntity.Value, archetype, row);

        _entityRecords.Remove(entity);
        _entityManager.Destroy(entity);
        return true;
    }

    public bool IsAlive(Entity entity) => _entityManager.IsAlive(entity);

    public void AddComponent<T>(Entity entity, T component) where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var oldArchetype, out var oldRow))
            throw new ArgumentException("Entity does not exist or has been destroyed");

        var oldType = oldArchetype.Type;
        var componentType = typeof(T);

        if (oldType.Contains(componentType))
        {
            oldArchetype.Get<T>(oldRow) = component;
            return;
        }

        // Create new archetype type with the component added
        var newComponentTypes = oldType.ComponentTypes.Append(componentType).ToArray();
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
            var oldCompType = oldType.ComponentTypes[i];
            newComponents[typeToNewIndex[oldCompType]] = oldComponents[i];
        }

        // Add new component
        newComponents[typeToNewIndex[componentType]] = component!;

        // Move entity to new archetype
        var movedEntity = oldArchetype.RemoveRow(oldRow);
        if (movedEntity.HasValue)
            _entityRecords.SetLocation(movedEntity.Value, oldArchetype, oldRow);

        var newRow = newArchetype.RowCount;
        newArchetype.AddRow(entity, newComponents);
        _entityRecords.SetLocation(entity, newArchetype, newRow);
    }

    public void RemoveComponent<T>(Entity entity) where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var oldArchetype, out var oldRow))
            throw new ArgumentException("Entity does not exist or has been destroyed");

        var oldType = oldArchetype.Type;
        var componentType = typeof(T);

        if (!oldType.Contains(componentType))
            throw new InvalidOperationException($"Entity does not have component {typeof(T).Name}");

        // Create new archetype type without the component
        var newComponentTypes = oldType.ComponentTypes.Where(t => t != componentType).ToArray();
        Archetype newArchetype = newComponentTypes.Length == 0
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
            if (oldCompType != componentType && oldToNewIndex.TryGetValue(oldCompType, out var newIdx))
                newComponents[newIdx] = oldComponents[i];
        }

        // Move entity to new archetype
        var movedEntity = oldArchetype.RemoveRow(oldRow);
        if (movedEntity.HasValue)
            _entityRecords.SetLocation(movedEntity.Value, oldArchetype, oldRow);

        var newRow = newArchetype.RowCount;
        newArchetype.AddRow(entity, newComponents);
        _entityRecords.SetLocation(entity, newArchetype, newRow);
    }

    public ref T GetComponent<T>(Entity entity) where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var archetype, out var row))
            throw new ArgumentException("Entity does not exist or has been destroyed");
        return ref archetype.Get<T>(row);
    }

    public bool HasComponent<T>(Entity entity) where T : struct
    {
        if (!_entityRecords.TryGetLocation(entity, out var archetype, out _))
            return false;
        return archetype.Type.Contains(typeof(T));
    }

    // ========================================================================
    // CACHE-EFFICIENT ITERATION
    // ========================================================================

    public void ForEach<T1>(RefAction<T1> action) where T1 : struct
    {
        var componentType = typeof(T1);
        var archetypes = _archetypeManager.GetArchetypesWith(componentType);
        if (archetypes == null) return;

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
        var archetypes1 = _archetypeManager.GetArchetypesWith(type1);
        if (archetypes1 == null) return;

        foreach (var archetype in archetypes1)
        {
            if (!archetype.Type.Contains(type2))
                continue;
            if (!archetype.TryGetColumn(type1, out var column1) ||
                !archetype.TryGetColumn(type2, out var column2))
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
        var archetypes1 = _archetypeManager.GetArchetypesWith(type1);
        if (archetypes1 == null) return;

        foreach (var archetype in archetypes1)
        {
            if (!archetype.Type.Contains(type2) || !archetype.Type.Contains(type3))
                continue;
            if (!archetype.TryGetColumn(type1, out var column1) ||
                !archetype.TryGetColumn(type2, out var column2) ||
                !archetype.TryGetColumn(type3, out var column3))
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
        var archetypes1 = _archetypeManager.GetArchetypesWith(type1);
        if (archetypes1 == null) return;

        foreach (var archetype in archetypes1)
        {
            if (!archetype.Type.Contains(type2) ||
                !archetype.Type.Contains(type3) ||
                !archetype.Type.Contains(type4))
                continue;
            if (!archetype.TryGetColumn(type1, out var column1) ||
                !archetype.TryGetColumn(type2, out var column2) ||
                !archetype.TryGetColumn(type3, out var column3) ||
                !archetype.TryGetColumn(type4, out var column4))
                continue;
            var col1Array = (T1[])column1;
            var col2Array = (T2[])column2;
            var col3Array = (T3[])column3;
            var col4Array = (T4[])column4;
            for (var row = 0; row < archetype.RowCount; row++)
            {
                var entity = archetype.GetEntityAt(row);
                action(entity, ref col1Array[row], ref col2Array[row], ref col3Array[row], ref col4Array[row]);
            }
        }
    }

    // ========================================================================
    // ENTITY QUERY METHODS
    // ========================================================================

    public IReadOnlyList<Entity> GetEntitiesWith<T>() where T : struct
    {
        var componentType = typeof(T);
        var archetypes = _archetypeManager.GetArchetypesWith(componentType);
        if (archetypes == null) return Array.Empty<Entity>();
        var entities = new List<Entity>();
        foreach (var archetype in archetypes)
        {
            for (var row = 0; row < archetype.RowCount; row++)
                entities.Add(archetype.GetEntityAt(row));
        }
        return entities;
    }

    public int GetEntitiesWith<T>(Span<Entity> buffer) where T : struct
    {
        var componentType = typeof(T);
        var archetypes = _archetypeManager.GetArchetypesWith(componentType);
        if (archetypes == null) return 0;
        var count = 0;
        foreach (var archetype in archetypes)
        {
            for (var row = 0; row < archetype.RowCount; row++)
            {
                if (count >= buffer.Length) goto Done;
                buffer[count++] = archetype.GetEntityAt(row);
            }
        }
        Done:
        return count;
    }

    public IReadOnlyList<Entity> GetEntitiesWith<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var archetypes1 = _archetypeManager.GetArchetypesWith(type1);
        if (archetypes1 == null) return Array.Empty<Entity>();
        var entities = new List<Entity>();
        foreach (var archetype in archetypes1)
        {
            if (!archetype.Type.Contains(type2)) continue;
            for (var row = 0; row < archetype.RowCount; row++)
                entities.Add(archetype.GetEntityAt(row));
        }
        return entities;
    }

    public int GetEntitiesWith<T1, T2>(Span<Entity> buffer)
        where T1 : struct
        where T2 : struct
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var archetypes1 = _archetypeManager.GetArchetypesWith(type1);
        if (archetypes1 == null) return 0;
        var count = 0;
        foreach (var archetype in archetypes1)
        {
            if (!archetype.Type.Contains(type2)) continue;
            for (var row = 0; row < archetype.RowCount; row++)
            {
                if (count >= buffer.Length) goto Done;
                buffer[count++] = archetype.GetEntityAt(row);
            }
        }
        Done:
        return count;
    }

    public IReadOnlyList<Entity> GetEntitiesWith<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        var types = new[] { typeof(T1), typeof(T2), typeof(T3) };
        return GetEntitiesWithInternal(types);
    }

    public IReadOnlyList<Entity> GetEntitiesWith<T1, T2, T3, T4>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        var types = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
        return GetEntitiesWithInternal(types);
    }

    public IReadOnlyList<Entity> GetEntitiesWith(params Type[] componentTypes)
        => GetEntitiesWithInternal(componentTypes);

    private IReadOnlyList<Entity> GetEntitiesWithInternal(Type[] componentTypes)
    {
        if (componentTypes.Length == 0)
            return Array.Empty<Entity>();

        var smallestSet = _archetypeManager.GetArchetypesWith(componentTypes);
        if (smallestSet == null)
            return Array.Empty<Entity>();

        var entities = new List<Entity>();
        foreach (var archetype in smallestSet)
        {
            var hasAll = true;
            foreach (var type in componentTypes)
            {
                if (!archetype.Type.Contains(type))
                {
                    hasAll = false;
                    break;
                }
            }
            if (hasAll)
            {
                for (var row = 0; row < archetype.RowCount; row++)
                    entities.Add(archetype.GetEntityAt(row));
            }
        }
        return entities;
    }

    // ========================================================================
    // DISPOSAL
    // ========================================================================

    public void Dispose()
    {
        _archetypeManager.Dispose();
        _componentRecord.Dispose();
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
                if (i > 0) sb.Append(", ");
                sb.Append(type.ComponentTypes[i].Name);
            }
            sb.Append($"): {archetype.RowCount} entities");
        }
        return sb.ToString();
    }
}
