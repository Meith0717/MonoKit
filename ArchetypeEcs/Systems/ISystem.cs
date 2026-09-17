// ISystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace MonoKit.ArchetypeEcs.Systems;

public interface ISystem : IDisposable
{
	void Initialize(World world);
}

public interface IUpdateSystem : ISystem
{
	void Update(
		double elapsedMs,
		World world,
		RuntimeContainer runtimeServices,
		InputHandler inputHandler
	);
}

public interface IDrawSystem : ISystem
{
	void Draw(double elapsedMs, World world);
}
