// GameScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.Game.Camera;

namespace MonoKit.Game.Screens;

/// <summary>
/// Screen hosting a <see cref="GameHost"/> in 2D or 3D. Rendering runs in the world's draw
/// systems. Override <see cref="Initialize"/> to spawn content and <see cref="Update"/> for
/// input/game logic. Cameras: <see cref="GameHost.Camera2D"/> / <see cref="GameHost.Camera3D"/>.
/// </summary>
public class GameScreen : IScreen
{
    public GameHost Host { get; }

    /// <summary>The host's camera, dimension-agnostic.</summary>
    protected ICamera Camera => Host.Camera;
    public bool UpdateBelow { get; }
    public bool DrawBelow { get; }

    public GameScreen(
        GameServiceContainer services,
        RuntimeDimension dimension,
        bool updateBelow = false,
        bool drawBelow = true
    )
    {
        Host = new GameHost(services.GetService<GraphicsDevice>(), dimension);
        UpdateBelow = updateBelow;
        DrawBelow = drawBelow;
    }

    public virtual void Initialize() { }

    public virtual void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        Host.Update(elapsedMs, inputHandler);
    }

    public virtual void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        Host.Draw(spriteBatch, graphicsDevice);
    }

    public virtual void OnResolutionChanged(float uiScale) { }

    public virtual void Dispose()
    {
        Host.Dispose();
    }
}
