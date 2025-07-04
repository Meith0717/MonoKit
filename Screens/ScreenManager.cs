// ScreenManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using GameEngine.Graphics;
using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Screens;

public class ScreenManager(Game game)
{
    private readonly Game _game = game;

    // layer stack
    private readonly LinkedList<Screen> _screenStack = new();
    private readonly List<Screen> _addedScreens = new();
    private RenderTarget2D _lowerScreens;
    private Effect _blurEffect;
    private float _blurIntensity;
    private float _topLayerAlpha;

    public void Initialize()
    {
        _blurEffect = ContentProvider.Effects.Get("GaussianBlur");
    }

    // add and remove layers from stack
    public void AddScreen(Screen layer) => _addedScreens.Add(layer);

    public void PopScreen()
    {
        if (_screenStack.First is null) return;
        _screenStack.First.Value.Dispose();
        _screenStack.RemoveFirst();
    }

    public void PopScreensUntil(Screen layer)
    {
        if (layer == null) return;
        if (!_screenStack.Contains(layer)) return;
        Screen firstLayer = _screenStack.First();
        while (firstLayer != layer)
        {
            PopScreen();
            firstLayer = _screenStack.First();
        }
        PopScreen();
    }

    // update layers
    public void Update(GameTime gameTime, InputState inputState, float uiScale)
    {
        List<Screen> addedLayers = _addedScreens.ToList();
        _addedScreens.Clear();
        foreach (Screen layer in addedLayers)
        {
            _screenStack.AddFirst(layer);
            layer.Initialize();
            layer.ApplyResolution(gameTime, uiScale);
            _topLayerAlpha = 0;
        }

        foreach (Screen layer in _screenStack.ToList())
        {
            layer.Update(gameTime, inputState);
            if (!layer.UpdateBelow) break;
        }

        var topScreen = _screenStack.FirstOrDefault();
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
        if (_screenStack.Count == 0) return;

        var topScreen = _screenStack.First();
        var topScreenTexture = topScreen.RenderTarget(spriteBatch);

        if (topScreen.DrawBelow)
        {
            // Render all renderTargets of lower screens
            foreach (var screen in _screenStack)
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
        foreach (Screen layer in _screenStack)
            layer.Dispose();
        _game.Exit();
    }

    // fullScreen stuff
    public void OnResolutionChanged(GameTime gameTime, float uiScale)
    {
        _lowerScreens?.Dispose();
        foreach (Screen layer in _screenStack)
            layer.ApplyResolution(gameTime, uiScale);
        _lowerScreens = new(_game.GraphicsDevice,
                            _game.GraphicsDevice.Viewport.Width,
                            _game.GraphicsDevice.Viewport.Height,
                            false,
                            SurfaceFormat.HdrBlendable,
                           DepthFormat.None);
        _blurEffect.Parameters["texelSize"].SetValue(new Vector2(1.0f / _lowerScreens.Width, 1.0f / _lowerScreens.Height));
    }

    public bool ContainsLayer(Screen layer) => _screenStack.Contains(layer);
}