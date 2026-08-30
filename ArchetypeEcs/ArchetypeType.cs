// ArchetypeType.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Linq;

namespace MonoKit.ArchetypeEcs;

public readonly struct ArchetypeType : IEquatable<ArchetypeType>
{
    public readonly Type[] ComponentTypes;

    public ArchetypeType(params Type[] componentTypes)
    {
        ComponentTypes = [.. componentTypes.OrderBy(t => t.FullName)];
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

    public bool Contains(Type componentType)
    {
        var key = componentType.FullName;
        var left = 0;
        var right = ComponentTypes.Length - 1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var comparison = string.Compare(
                ComponentTypes[mid].FullName,
                key,
                StringComparison.Ordinal
            );

            switch (comparison)
            {
                case 0:
                    return true;
                case < 0:
                    left = mid + 1;
                    break;
                default:
                    right = mid - 1;
                    break;
            }
        }

        return false;
    }

    public int Count => ComponentTypes.Length;

    public static ArchetypeType Empty => new();

    public static bool operator ==(ArchetypeType left, ArchetypeType right) => left.Equals(right);

    public static bool operator !=(ArchetypeType left, ArchetypeType right) => !(left == right);
}
