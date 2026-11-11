// MonoKit - ArchetypeType.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Linq;

namespace MonoKit.Game.ArchetypeEcs.Archetypes;

internal readonly struct ArchetypeType : IEquatable<ArchetypeType>
{
    internal static readonly ArchetypeType Empty = new([]);
    internal readonly Type[] ComponentTypes = [];

    /// <summary>Canonicalizes the component types into a stable identity.</summary>
    internal ArchetypeType(params Type[] componentTypes)
    {
        ComponentTypes = [.. componentTypes.OrderBy(t => t.FullName, StringComparer.Ordinal)];
    }

    /// <summary>Compares the component type sequences for equality.</summary>
    public bool Equals(ArchetypeType other)
    {
        return ComponentTypes.SequenceEqual(other.ComponentTypes);
    }

    /// <summary>Compares against another boxed archetype type.</summary>
    public override bool Equals(object obj)
    {
        return obj is ArchetypeType other && Equals(other);
    }

    /// <summary>Combines the component types into a hash code.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var type in ComponentTypes)
            hash.Add(type);
        return hash.ToHashCode();
    }

    /// <summary>Checks whether two archetype types are equal.</summary>
    public static bool operator ==(ArchetypeType left, ArchetypeType right)
    {
        return left.Equals(right);
    }

    /// <summary>Checks whether two archetype types differ.</summary>
    public static bool operator !=(ArchetypeType left, ArchetypeType right)
    {
        return !(left == right);
    }
}
