// AppHost - Entity.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace MonoKit.Game.ArchetypeEcs.Entities;

public readonly record struct Entity(int Id, int Generation)
{
    public readonly int Generation = Generation;
    public readonly int Id = Id;
}
