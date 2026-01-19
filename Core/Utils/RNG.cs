// RNG.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace MonoKit.Core.Utils;

public static class RNG
{
    public static readonly Random Random = new();

    public static bool NextBool(this Random random)
    {
        return random.NextDouble() < 0.5;
    }
}
