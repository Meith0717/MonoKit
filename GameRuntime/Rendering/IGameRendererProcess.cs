// IGameRendererProcess.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.GameRuntime.Ecs;
using MonoKit.GameRuntime.Ecs.Entities;

namespace MonoKit.GameRuntime.Rendering;

public interface IEcsGameRendererProcess
{
    void UpdateEffects(double elapsedMilliseconds);

    void DrawEffects(
        SpriteBatch spriteBatch,
        Matrix transformMatrix,
        World world,
        IReadOnlyList<Entity> entities
    );

    void DrawTextures(SpriteBatch spriteBatch, World world, IReadOnlyList<Entity> entities);
}
