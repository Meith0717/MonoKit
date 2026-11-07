// AppHost - ArchetypeType.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Linq;

namespace MonoKit.GameRuntime.ArchetypeEcs.Archetypes;

internal readonly struct ArchetypeType : IEquatable<ArchetypeType>
{
    internal static readonly ArchetypeType Empty = new([]);
    internal readonly Type[] ComponentTypes = [];

    internal ArchetypeType(params Type[] componentTypes)
    {
        ComponentTypes = [.. componentTypes.OrderBy(t => t.FullName, StringComparer.Ordinal)];
    }

    public bool Equals(ArchetypeType other)
    {
        return ComponentTypes.SequenceEqual(other.ComponentTypes);
    }

    public override bool Equals(object obj)
    {
        return obj is ArchetypeType other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var type in ComponentTypes)
            hash.Add(type);
        return hash.ToHashCode();
    }

    public static bool operator ==(ArchetypeType left, ArchetypeType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArchetypeType left, ArchetypeType right)
    {
        return !(left == right);
    }
}
