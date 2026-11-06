// ScreenManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Graphics.Rendering;
using MonoKit.App.Input;

namespace MonoKit.App.Screens;

public class ScreenManager(Game game)
{
    private readonly GraphicsDevice _graphicsDevice = game.GraphicsDevice;

    private readonly LinkedList<RenderTarget2D> _lowerRenderTargets = new();
    private readonly ConcurrentQueue<Action<double, float>> _pendingActions = new();
    private readonly PostProcessingRunner _postProcessingRunner = new(game.GraphicsDevice);
    private readonly Stack<Screen> _screens = new();
    private RenderTarget2D _lowerRenderTarget;
    private bool _runEffect;
    public IPostProcessingEffect PostProcessingEffect;

    public void AddScreen(Screen screen)
    {
        _pendingActions.Enqueue(
            (elapsedMilliseconds, uiScale) =>
            {
                _screens.Push(screen);
                screen.Initialize();
                screen.ApplyResolution(elapsedMilliseconds, uiScale);
            }
        );
    }

    public void PopScreen()
    {
        _pendingActions.Enqueue(
            (_, _) =>
            {
                var removedScreen = _screens.Pop();
                removedScreen?.Dispose();
            }
        );
    }

    public void PopScreensUntil(Screen screen)
    {
        if (!_screens.Contains(screen))
            return;

        for (var i = 0; i < _screens.Count; i++)
        {
            PopScreen();
            if (_screens.ElementAt(i) == screen)
                break;
        }
    }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler, float uiScale)
    {
        while (_pendingActions.TryDequeue(out var action))
            action(elapsedMilliseconds, uiScale);

        for (var i = 0; i < _screens.Count; i++)
        {
            var screen = _screens.ElementAt(i);
            screen.Update(elapsedMilliseconds, inputHandler, uiScale);
            if (!screen.UpdateBelow)
                break;
        }
    }

    public void EffectOn()
    {
        _runEffect = true;
    }

    public void EffectOff()
    {
        _runEffect = false;
    }

    public void DrawWithRenderTarget(SpriteBatch spriteBatch)
    {
        if (_screens.Count == 0)
            return;
        _lowerRenderTargets.Clear();

        var topScreenTarget = _screens.First().RenderTarget(spriteBatch);

        var i = 0;
        while (_screens.ElementAt(i).DrawBelow)
            i++;

        for (var j = i; j > 0; j--)
            _lowerRenderTargets.AddFirst(_screens.ElementAt(j).RenderTarget(spriteBatch));

        _graphicsDevice.SetRenderTarget(_lowerRenderTarget);
        game.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        foreach (var lowerTarget in _lowerRenderTargets)
            spriteBatch.Draw(lowerTarget, Vector2.Zero, Color.White);
        spriteBatch.End();

        if (_runEffect)
            _lowerRenderTarget = PostProcessingEffect?.Apply(
                spriteBatch,
                _postProcessingRunner,
                _lowerRenderTarget
            );

        _graphicsDevice.SetRenderTarget(null);
        game.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        spriteBatch.Draw(_lowerRenderTarget, Vector2.Zero, Color.White);
        spriteBatch.Draw(topScreenTarget, Vector2.Zero, Color.White);
        spriteBatch.End();
    }

    private readonly List<Screen> _filteredScreens = [];

    public void Draw(SpriteBatch spriteBatch)
    {
        _filteredScreens.Clear();
        foreach (var screen in _screens)
        {
            _filteredScreens.Add(screen);
            if (!screen.DrawBelow)
                break;
        }

        for (var i = _filteredScreens.Count - 1; i >= 0; i--)
            _filteredScreens[i].Draw(spriteBatch);
    }

    public void Exit()
    {
        for (var i = 0; i < _screens.Count; i++)
            _screens.ElementAt(i).Dispose();

        _postProcessingRunner.Dispose();
        _lowerRenderTarget?.Dispose();
        game.Exit();
    }

    public void OnResolutionChanged(double elapsedMilliseconds, float uiScale)
    {
        foreach (var layer in _screens)
            layer.ApplyResolution(elapsedMilliseconds, uiScale);
        _postProcessingRunner.ApplyResolution(
            _graphicsDevice.Viewport.Width,
            _graphicsDevice.Viewport.Height
        );
        _lowerRenderTarget?.Dispose();
        _lowerRenderTarget = new RenderTarget2D(
            _graphicsDevice,
            _graphicsDevice.Viewport.Width,
            _graphicsDevice.Viewport.Height
        );
    }
}
