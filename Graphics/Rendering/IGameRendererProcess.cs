// IGameRendererProcess.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Ecs.Entities;
using MonoKit.Gameplay;

namespace MonoKit.Graphics.Rendering;

public interface IGameRendererProcess
{
    void UpdateEffects(double elapsedMilliseconds);

    void DrawEffects(
        SpriteBatch spriteBatch,
        Matrix transformMatrix,
        RuntimeContainer services,
        IReadOnlyList<GameObject> gameObjects
    );

    void DrawTextures(
        SpriteBatch spriteBatch,
        RuntimeContainer services,
        IReadOnlyList<GameObject> gameObjects
    );
}

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
