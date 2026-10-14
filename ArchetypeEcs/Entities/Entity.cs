// Entity.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace MonoKit.ArchetypeEcs.Entities;

public readonly record struct Entity(int Id, int Generation)
{
    public readonly int Id = Id;
    public readonly int Generation = Generation;
}
