// Entity.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace MonoKit.ArchetypeEcs.Entities;

/// <summary>
/// A handle to an entity. <see cref="Id"/> identifies the slot, <see cref="Generation"/>
/// distinguishes reuses of a recycled slot: a handle from a destroyed entity never matches
/// a newer entity created with the same id.
/// </summary>
public readonly record struct Entity(int Id, int Generation)
{
    public readonly int Id = Id;
    public readonly int Generation = Generation;
}
