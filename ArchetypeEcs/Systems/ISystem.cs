// ISystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace MonoKit.ArchetypeEcs.Systems;

public interface ISystem : IDisposable
{
    void Initialize(World world);
}

public interface IUpdateSystem : ISystem
{
    void Update(double elapsedMs, World world);
}

public interface IDrawSystem : ISystem
{
    void Draw(double elapsedMs, World world);
}
