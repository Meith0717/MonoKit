// IRenderer2DProcess.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Gameplay;

namespace MonoKit.Graphics.Rendering;

public interface IRenderer2DProcess
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
