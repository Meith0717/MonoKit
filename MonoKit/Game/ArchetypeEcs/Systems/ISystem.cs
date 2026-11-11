// MonoKit - ISystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.Game.ArchetypeEcs.Systems;

public interface ISystem : IDisposable
{
    /// <summary>Initializes the system within the given world.</summary>
    void Initialize(World world);
}

public interface IUpdateSystem : ISystem
{
    /// <summary>Advances the system by the elapsed milliseconds.</summary>
    void Update(double elapsedMs, World world);
}

public interface IDrawSystem : ISystem
{
    /// <summary>Draws the system using the given sprite batch.</summary>
    void Draw(
        double elapsedMs,
        World world,
        SpriteBatch spriteBatch,
        GraphicsDevice graphicsDevice
    );
}
