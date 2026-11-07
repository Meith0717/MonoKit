// AppHost - ScreenManager.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;
using MonoKit.App.Rendering;

namespace MonoKit.App.Screens;

/// <summary>
///     Stack of <see cref="IScreen" />s. Updates top-down, composites the visible lower chain
///     with an optional effect, and draws the top screen unmodified on top.
/// </summary>
public class ScreenManager(Game game)
{
    private readonly ConcurrentQueue<Action<float>> _pendingActions = new();
    private readonly PostProcessingRunner _postProcessingRunner = new();
    private readonly Stack<IScreen> _screens = new();
    private RenderTarget2D _lowerComposite;
    private RenderTarget2D _screenScratch;

    /// <summary>Applied to the chain below the top screen, for example a blur behind a menu.</summary>
    public IPostProcessingEffect PostProcessingEffect { get; init; }

    /// <summary>Set to false to render the lower chain unblurred, for example during a transition.</summary>
    public bool EffectEnabled { get; set; } = true;

    /// <summary>Pushes a screen onto the stack; applied at the start of the next update.</summary>
    public void AddScreen(IScreen screen)
    {
        _pendingActions.Enqueue(uiScale =>
        {
            _screens.Push(screen);
            screen.Initialize();
            screen.OnResolutionChanged(uiScale);
        });
    }

    /// <summary>Pops and disposes the top screen; applied at the start of the next update.</summary>
    public void PopScreen()
    {
        _pendingActions.Enqueue(_ =>
        {
            if (_screens.Count == 0)
                return;
            _screens.Pop().Dispose();
        });
    }

    /// <summary>Pops every screen above the given screen; that screen stays on top.</summary>
    public void PopScreensUntil(IScreen screen)
    {
        _pendingActions.Enqueue(_ =>
        {
            if (!_screens.Contains(screen))
                return;
            while (_screens.Peek() != screen)
                _screens.Pop().Dispose();
        });
    }

    /// <summary>Applies pending push/pop actions, then updates the stack top-down.</summary>
    public void Update(double elapsedMilliseconds, InputHandler inputHandler, float uiScale)
    {
        while (_pendingActions.TryDequeue(out var action))
            action(uiScale);

        // Stack<T> enumerates top-down: the topmost screen updates first.
        foreach (var screen in _screens)
        {
            screen.Update(elapsedMilliseconds, inputHandler, uiScale);
            if (!screen.UpdateBelow)
                break;
        }
    }

    private readonly List<IScreen> _visibleChain = [];

    /// <summary>Draws the visible screen chain: lower screens composited (optionally with the effect), top screen last.</summary>
    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        _postProcessingRunner.BeginFrame();

        if (_screens.Count == 0)
            return;

        var top = _screens.Peek();

        // Visible chain below the top screen: every screen up to and
        // including the first one with DrawBelow = false. Everything below
        // that screen is hidden and is not rendered at all.
        _visibleChain.Clear();
        var skipTop = true;
        foreach (var screen in _screens)
        {
            if (skipTop)
            {
                skipTop = false;
                continue;
            }

            _visibleChain.Add(screen);
            if (!screen.DrawBelow)
                break;
        }

        // Fast path: nothing below the top screen, no compositing needed.
        if (_visibleChain.Count == 0)
        {
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.Black);
            RenderScreen(spriteBatch, graphicsDevice, top);
            return;
        }

        // 1. Render the lower chain into the composite target, bottom-most
        //    screen first, so blending order is preserved.
        var composite = GetOrCreateComposite(graphicsDevice);
        graphicsDevice.SetRenderTarget(composite);
        graphicsDevice.Clear(Color.Transparent);
        for (var i = _visibleChain.Count - 1; i >= 0; i--)
            RenderScreen(spriteBatch, graphicsDevice, _visibleChain[i]);

        // 2. Apply the manager-wide effect (for example a blur). The result
        //    goes into a local; the field is never reassigned.
        if (EffectEnabled && PostProcessingEffect is not null)
            composite = PostProcessingEffect.Apply(
                spriteBatch,
                graphicsDevice,
                _postProcessingRunner,
                composite
            );

        // 3. Composite to the back buffer.
        graphicsDevice.SetRenderTarget(null);
        graphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        spriteBatch.Draw(composite, Vector2.Zero, Color.White);
        spriteBatch.End();

        // 4. Top screen last and unblurred, straight to the back buffer.
        RenderScreen(spriteBatch, graphicsDevice, top);
    }

    /// <summary>
    ///     Draws a screen. Plain screens draw directly into the currently bound
    ///     target. Screens implementing IPostProcessedScreen are rendered into
    ///     the shared scratch target first, their effect is applied, and the
    ///     result is composited back onto the previous binding.
    /// </summary>
    private void RenderScreen(
        SpriteBatch spriteBatch,
        GraphicsDevice graphicsDevice,
        IScreen screen
    )
    {
        if (screen is not IPostProcessedScreen processed)
        {
            screen.Draw(spriteBatch, graphicsDevice);
            return;
        }

        var previousBindings = graphicsDevice.GetRenderTargets();
        var scratch = GetOrCreateScreenScratch(graphicsDevice);

        graphicsDevice.SetRenderTarget(scratch);
        graphicsDevice.Clear(Color.Transparent);
        screen.Draw(spriteBatch, graphicsDevice);

        var result = processed.PostProcessingEffect.Apply(
            spriteBatch,
            graphicsDevice,
            _postProcessingRunner,
            scratch
        );

        graphicsDevice.SetRenderTargets(previousBindings);
        spriteBatch.Begin();
        spriteBatch.Draw(result, Vector2.Zero, Color.White);
        spriteBatch.End();
    }

    private RenderTarget2D GetOrCreateScreenScratch(GraphicsDevice graphicsDevice)
    {
        var width = graphicsDevice.Viewport.Width;
        var height = graphicsDevice.Viewport.Height;
        if (
            _screenScratch is null
            || _screenScratch.Width != width
            || _screenScratch.Height != height
        )
        {
            _screenScratch?.Dispose();
            // Depth buffer, because 3D screens (for example the game screen
            // with the glow pipeline) draw directly into this target.
            _screenScratch = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8
            );
        }

        return _screenScratch;
    }

    private RenderTarget2D GetOrCreateComposite(GraphicsDevice graphicsDevice)
    {
        var width = graphicsDevice.Viewport.Width;
        var height = graphicsDevice.Viewport.Height;
        if (
            _lowerComposite is null
            || _lowerComposite.Width != width
            || _lowerComposite.Height != height
        )
        {
            _lowerComposite?.Dispose();
            // Depth buffer, because 3D screens draw directly into this
            // target through the GraphicsDevice. SpriteBatch defaults to
            // DepthStencilState.None, so 2D screens are not affected.
            _lowerComposite = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8
            );
        }

        return _lowerComposite;
    }

    /// <summary>Disposes all screens and exits the game.</summary>
    public void Exit()
    {
        while (_screens.Count > 0)
            _screens.Pop().Dispose();
        _postProcessingRunner.Dispose();
        _lowerComposite?.Dispose();
        _screenScratch?.Dispose();
        game.Exit();
    }

    // Render targets recreate themselves lazily in Draw, so this method only
    // has to inform the screens and the runner about the new size.
    public void OnResolutionChanged(float uiScale, GraphicsDevice graphicsDevice)
    {
        foreach (var screen in _screens)
            screen.OnResolutionChanged(uiScale);
        _postProcessingRunner.ApplyResolution(
            graphicsDevice.Viewport.Width,
            graphicsDevice.Viewport.Height
        );
    }
}
