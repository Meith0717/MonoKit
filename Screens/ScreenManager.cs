// ScreenManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using GameEngine.Graphics;
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

    // layer stack
    private readonly Stack<Screen> _screens = new();
    private readonly ConcurrentQueue<Action<GameTime, float>> _pendingActions = new();
    private RenderTarget2D _lowerScreens;
    private Effect _blurEffect;
    private float _blurIntensity;
    private float _topLayerAlpha;

    public void Initialize()
    {
        _blurEffect = ContentProvider.Effects.Get("GaussianBlur");
    }

    // add and remove layers from stack
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

    // update layers
    public void Update(GameTime gameTime, InputState inputState, float uiScale)
    {
        while (_pendingActions.TryDequeue(out var action))
        {
            action(gameTime, uiScale);
            _topLayerAlpha = 0;
        }

        foreach (Screen layer in _screens.ToList())
        {
            layer.Update(gameTime, inputState, uiScale);
            if (!layer.UpdateBelow) break;
        }

        var topScreen = _screens.FirstOrDefault();
        if (topScreen == null) return;
        _topLayerAlpha = float.Min(1, _topLayerAlpha + (float)(.005 * gameTime.ElapsedGameTime.TotalMilliseconds));
        _blurIntensity += (topScreen.BlurBelow ? 1 : -1) * (float)(.075 * gameTime.ElapsedGameTime.TotalMilliseconds);
        _blurIntensity = float.Clamp(_blurIntensity, .01f, 10);
        _blurEffect.Parameters["kernel"].SetValue(GaussianBlur.GetGaussianKernel1D(20, _blurIntensity));
    }

    // draw layers
    private readonly LinkedList<RenderTarget2D> _renderTargets = new();

    public void Draw(SpriteBatch spriteBatch)
    {
        _renderTargets.Clear();
        if (_screens.Count == 0) return;

        var topScreen = _screens.First();
        var topScreenTexture = topScreen.RenderTarget(spriteBatch);

        if (topScreen.DrawBelow)
        {
            // Render all renderTargets of lower screens
            foreach (var screen in _screens)
            {
                if (screen == topScreen) continue;
                _renderTargets.AddFirst(screen.RenderTarget(spriteBatch));
                if (!screen.DrawBelow) break;
            }

            // Set draw all on lower renderTarget
            _blurEffect.CurrentTechnique = _blurEffect.Techniques["GaussianBlurH"];
            _game.GraphicsDevice.SetRenderTarget(_lowerScreens);
            _game.GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(effect: topScreen.BlurBelow ? _blurEffect : null);
            foreach (var screen in _renderTargets)
                spriteBatch.Draw(screen, Vector2.Zero, Color.White);
            spriteBatch.End();

            _blurEffect.CurrentTechnique = _blurEffect.Techniques["GaussianBlurV"];
            _game.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(effect: topScreen.BlurBelow ? _blurEffect : null);
            spriteBatch.Draw(_lowerScreens, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        spriteBatch.Begin();
        spriteBatch.Draw(topScreenTexture, Vector2.Zero, Color.White * _topLayerAlpha);
        spriteBatch.End();
    }

    // lifecycle methods
    public void Exit()
    {
        foreach (Screen layer in _screens)
            layer.Dispose();
        _game.Exit();
    }

    // fullScreen stuff
    public void OnResolutionChanged(GameTime gameTime, float uiScale)
    {
        _lowerScreens?.Dispose();
        foreach (Screen layer in _screens)
            layer.ApplyResolution(gameTime, uiScale);
        _lowerScreens = new(_game.GraphicsDevice,
                            _game.GraphicsDevice.Viewport.Width,
                            _game.GraphicsDevice.Viewport.Height,
                            false,
                            SurfaceFormat.HdrBlendable,
                           DepthFormat.None);
        _blurEffect.Parameters["texelSize"].SetValue(new Vector2(1.0f / _lowerScreens.Width, 1.0f / _lowerScreens.Height));
    }
}