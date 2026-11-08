// AppHost - IScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;

namespace MonoKit.App.Screens;

/// <summary>A screen managed by <see cref="ScreenManager" />: updated and drawn as part of a stack.</summary>
public interface IScreen : IDisposable
{
    /// <summary>When true, screens below this one keep updating.</summary>
    public bool UpdateBelow { get; }

    /// <summary>When true, screens below this one remain part of the rendered chain.</summary>
    public bool DrawBelow { get; }

    /// <summary>Called once when the screen is pushed onto the stack.</summary>
    public void Initialize();

    /// <summary>Advances the screen. Skipped when a screen above has <see cref="UpdateBelow" /> set to false.</summary>
    public void Update(double elapsedMs, InputHandler inputHandler, float uiScale);

    /// <summary>Draws the screen. The device is passed for screens doing 3D rendering.</summary>
    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice);

    /// <summary>Reapplies layout after the window size or UI scale changed.</summary>
    public void OnResolutionChanged(float uiScale);
}
