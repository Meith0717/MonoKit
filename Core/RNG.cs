﻿// RNG.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;

namespace MonoKit.Core
{
    public static class RNG
    {
        public static readonly Random Random = new();

        public static bool NextBool(this Random random)
            => random.NextDouble() < 0.5;
    }
}
