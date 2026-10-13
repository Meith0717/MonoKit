// ArchetypeType.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Linq;

namespace MonoKit.ArchetypeEcs.Archtypes;

internal readonly struct ArchetypeType : IEquatable<ArchetypeType>
{
    internal static ArchetypeType Empty = new([]);
    internal readonly Type[] ComponentTypes = [];

    internal ArchetypeType(params Type[] componentTypes)
    {
        ComponentTypes = [.. componentTypes.OrderBy(t => t.FullName, StringComparer.Ordinal)];
    }

    public bool Equals(ArchetypeType other) => ComponentTypes.SequenceEqual(other.ComponentTypes);

    public override bool Equals(object obj) => obj is ArchetypeType other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var type in ComponentTypes)
            hash.Add(type);
        return hash.ToHashCode();
    }

    public static bool operator ==(ArchetypeType left, ArchetypeType right) => left.Equals(right);

    public static bool operator !=(ArchetypeType left, ArchetypeType right) => !(left == right);
}
