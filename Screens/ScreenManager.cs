// ScreenManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Core;
using GameEngine.Graphics;
using GameEngine.Input;
using GameEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Screens;

public class ScreenManager(Game game)
{
    private readonly Game _game = game;
    private readonly GraphicsDevice _graphicsDevice = game.GraphicsDevice;
    private readonly Stack<Screen> _screens = new();
    private readonly ConcurrentQueue<Action<double, float>> _pendingActions = new();

    private readonly LinkedList<RenderTarget2D> _lowerRenderTargets = new();
    private readonly PostProcessingRunner _postProcessingRunner = new(game.GraphicsDevice);
    private RenderTarget2D _lowerRenderTarget;
    private bool _runnEffect;
    public IPostProcessingEffect PostProcessingEffect;

    public void AddScreen(Screen screen)
    {
        _pendingActions.Enqueue((gT, uI) =>
        {
            _screens.Push(screen);
            screen.Initialize();
            screen.ApplyResolution(gT, uI);
        });
    }

    public void PopScreen()
    {
        _pendingActions.Enqueue((_, _) =>
        {
            var removedScreen = _screens.Pop();
            removedScreen?.Dispose();
        });
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

    public void Update(double elapsedMilliseconds, InputState inputState, float uiScale)
    {
        while (_pendingActions.TryDequeue(out var action))
            action(elapsedMilliseconds, uiScale);

        for (int i = 0; i < _screens.Count; i++)
        {
            var screen = _screens.ElementAt(i);
            screen.Update(elapsedMilliseconds, inputState, uiScale);
            if (!screen.UpdateBelow)
                break;
        }
    }

    public void EffectOn() => _runnEffect = true;
    public void EffectOff() => _runnEffect = false;

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_screens.Count == 0) return;
        _lowerRenderTargets.Clear();

        var topScreenTarget = _screens.First().RenderTarget(spriteBatch);

        int i = 0;
        while (_screens.ElementAt(i).DrawBelow) 
            i++;

        for (var j = i; j > 0; j--)
            _lowerRenderTargets.AddFirst(_screens.ElementAt(j).RenderTarget(spriteBatch));

        _graphicsDevice.SetRenderTarget(_lowerRenderTarget);
        _game.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        foreach (var lowerTarget in _lowerRenderTargets)
            spriteBatch.Draw(lowerTarget, Vector2.Zero, Color.White);
        spriteBatch.End();

        if (_runnEffect)
            _lowerRenderTarget = PostProcessingEffect?.Apply(spriteBatch, _postProcessingRunner, _lowerRenderTarget);

        _graphicsDevice.SetRenderTarget(null);
        _game.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        spriteBatch.Draw(_lowerRenderTarget, Vector2.Zero, Color.White);
        spriteBatch.Draw(topScreenTarget, Vector2.Zero, Color.White);
        spriteBatch.End();
    }

    public void Exit()
    {
        for (int i = 0; i < _screens.Count; i++)
            _screens.ElementAt(i).Dispose();

        _postProcessingRunner.Dispose();
        _lowerRenderTarget.Dispose();
        _game.Exit();
    }

    public void OnResolutionChanged(double elapsedMilliseconds, float uiScale)
    {
        foreach (Screen layer in _screens)
            layer.ApplyResolution(elapsedMilliseconds, uiScale);
        _postProcessingRunner.ApplyResolution(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        _lowerRenderTarget?.Dispose();
        _lowerRenderTarget = new(_graphicsDevice,
                              _graphicsDevice.Viewport.Width,
                              _graphicsDevice.Viewport.Height,
                              false,
                              SurfaceFormat.Color,
                              DepthFormat.Depth24Stencil8,
                              4,
                              RenderTargetUsage.PreserveContents);
    }
}