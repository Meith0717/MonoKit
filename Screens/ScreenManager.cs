// ScreenManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
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
    private readonly Stack<Screen> _screens = new();
    private readonly ConcurrentQueue<Action<GameTime, float>> _pendingActions = new();

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

    public void Update(GameTime gameTime, InputState inputState, float uiScale)
    {
        while (_pendingActions.TryDequeue(out var action))
            action(gameTime, uiScale);

        for (int i = 0; i < _screens.Count; i++)
        {
            var screen = _screens.ElementAt(i);
            screen.Update(gameTime, inputState, uiScale);
            if (!screen.UpdateBelow)
                break;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_screens.Count == 0)
            return;

        int i = 0;
        while (_screens.ElementAt(i).DrawBelow)
            i++;

        var topScreenTarget = _screens.First().RenderTarget(spriteBatch);
        var lowerTargets = new RenderTarget2D[i];

        for (var j = i; j > 0; j--)
            lowerTargets[j - 1] = _screens.ElementAt(j).RenderTarget(spriteBatch);

        _game.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        foreach (var lowerTarget in lowerTargets)
            spriteBatch.Draw(lowerTarget, Vector2.Zero, Color.White);
        spriteBatch.Draw(topScreenTarget, Vector2.Zero, Color.White);
        spriteBatch.End();
    }

    public void Exit()
    {
        for (int i = 0; i < _screens.Count; i++)
            _screens.ElementAt(i).Dispose();

        _game.Exit();
    }

    public void OnResolutionChanged(GameTime gameTime, float uiScale)
    {
        foreach (Screen layer in _screens)
            layer.ApplyResolution(gameTime, uiScale);
    }
}