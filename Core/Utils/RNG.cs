// RNG.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

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
