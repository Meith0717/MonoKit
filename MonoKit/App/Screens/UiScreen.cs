// AppHost - UiScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;
using MonoKit.App.Ui;

namespace MonoKit.App.Screens;

/// <summary>
///     Base class for UI-driven screens: owns a full-viewport root <see cref="UiFrame" /> that receives update, draw
///     and resize.
/// </summary>
public abstract class UiScreen(
    GameServiceContainer appServices,
    bool updateBelow = false,
    bool drawBelow = false
) : IScreen
{
    /// <summary>Root of the screen's UI tree.</summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly UiFrame UiRoot = new()
    {
        Color = Color.Transparent,
        RelWidth = 1,
        RelHeight = 1,
        FillScale = FillScale.FillIn,
        Align = Align.Center,
    };

    private readonly GraphicsDevice _graphicsDevice = appServices.GetService<GraphicsDevice>();

    public bool UpdateBelow => updateBelow;
    public bool DrawBelow => drawBelow;

    /// <summary>Called once on push; build the UI tree here.</summary>
    public abstract void Initialize();

    /// <summary>Updates the UI tree. Override to add custom logic.</summary>
    public virtual void Update(double elapsedMilliseconds, InputHandler inputHandler, float uiScale)
    {
        UiRoot.Update(inputHandler, _graphicsDevice.Viewport.Bounds, uiScale);
    }

    /// <summary>Draws the UI tree. Override to additionally draw 3D content via <paramref name="graphicsDevice" />.</summary>
    public virtual void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        spriteBatch.Begin();
        UiRoot.Draw(spriteBatch);
        spriteBatch.End();
    }

    /// <summary>Reapplies the UI scale to the root element.</summary>
    public virtual void OnResolutionChanged(float uiScale)
    {
        UiRoot.ApplyScale(_graphicsDevice.Viewport.Bounds, uiScale);
    }

    /// <summary>Disposes the UI tree. Override to release additional resources.</summary>
    public virtual void Dispose()
    {
        UiRoot.Dispose();
        GC.SuppressFinalize(this);
    }
}
