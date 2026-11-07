// AppHost - IGameRendererProcess.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

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
