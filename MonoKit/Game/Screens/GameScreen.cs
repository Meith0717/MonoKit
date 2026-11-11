// MonoKit - GameScreen.cs
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
///     Screen hosting a <see cref="GameHost" /> in 2D or 3D. Rendering runs in the world's draw
///     systems. Override <see cref="Initialize" /> to spawn content and <see cref="Update" /> for
///     input/game logic. Cameras: <see cref="GameHost.Camera2D" /> / <see cref="GameHost.Camera3D" />.
/// </summary>
public class GameScreen : IScreen
{
    /// <summary>Creates the screen and its game host for the given dimension.</summary>
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

    public GameHost Host { get; }

    /// <summary>The host's camera, dimension-agnostic.</summary>
    protected ICamera Camera => Host.Camera;

    public bool UpdateBelow { get; }
    public bool DrawBelow { get; }

    /// <summary>Hook: override to spawn world content when the screen initializes.</summary>
    public virtual void Initialize() { }

    /// <summary>Updates the game host; override to add game logic.</summary>
    public virtual void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        Host.Update(elapsedMs, inputHandler);
    }

    /// <summary>Draws the game host; override to add custom rendering.</summary>
    public virtual void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        Host.Draw(spriteBatch, graphicsDevice);
    }

    /// <summary>Hook: called when the window resolution changes.</summary>
    public virtual void OnResolutionChanged(float uiScale) { }

    /// <summary>Disposes the underlying game host.</summary>
    public virtual void Dispose()
    {
        Host.Dispose();
    }
}
