// GameScreen3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.GameRuntime.Gameplay;

namespace MonoKit.GameRuntime.Screens;

/// <summary>
/// Screen hosting a <see cref="GameRuntime3D"/>. The default Draw renders nothing —
/// override it to draw 3D content (models via BasicEffect/your pipeline) with
/// <see cref="GameRuntime3D.Camera"/>. Override <see cref="Initialize"/> to spawn
/// content and <see cref="Update"/> for input/game logic.
/// </summary>
public class GameScreen3D : IScreen
{
    public GameRuntime3D Runtime { get; }
    public bool UpdateBelow { get; }
    public bool DrawBelow { get; }

    public GameScreen3D(GameServiceContainer services, bool updateBelow = false, bool drawBelow = true)
    {
        Runtime = new GameRuntime3D(services.GetService<GraphicsDevice>());
        UpdateBelow = updateBelow;
        DrawBelow = drawBelow;
    }

    public virtual void Initialize() { }

    public virtual void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        Runtime.Update(elapsedMs, inputHandler);
    }

    public virtual void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        // 3D content is drawn by overrides; nothing to render by default.
    }

    public virtual void OnResolutionChanged(float uiScale) { }

    public virtual void Dispose()
    {
        Runtime.Dispose();
    }
}
