// ScreenManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
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
    private RenderTarget2D _blurRenderTarget;
    private Effect _blurEffect;

    public void Initialize()
    {
        _blurEffect = ContentProvider.Effects.Get("Blur");
        _blurEffect.Parameters["texelSize"].SetValue(new Vector2(1.0f / _game.GraphicsDevice.Viewport.Width, 1.0f / _game.GraphicsDevice.Viewport.Height));
        _blurRenderTarget = new(_game.GraphicsDevice, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height);
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
        }

        foreach (Screen layer in _screenStack.ToList())
        {
            layer.Update(gameTime, inputState);
            if (!layer.UpdateBelow) break;
        }
    }

    // draw layers
    private readonly LinkedList<RenderTarget2D> _renderTargets = new();
    private readonly Dictionary<RenderTarget2D, Effect> _layerEffects = new();
    public void Draw(SpriteBatch spriteBatch)
    {
        if (_screenStack.Count == 0) return;
        Screen topLayer = _screenStack.First();
        RenderTarget2D topRenderTarget = topLayer.RenderTarget(spriteBatch);

        if (topLayer.DrawBelow)
        {
            foreach (Screen layer in _screenStack)
            {
                if (layer == topLayer) continue;
                RenderTarget2D renderTarget = layer.RenderTarget(spriteBatch);
                _renderTargets.AddFirst(renderTarget);
                _layerEffects.Add(renderTarget, layer.Effect);
                if (!layer.DrawBelow) break;
            }

            // Set Blur Render Target
            _game.GraphicsDevice.SetRenderTarget(_blurRenderTarget);
            _game.GraphicsDevice.Clear(Color.Black);
            foreach (RenderTarget2D renderTarget in _renderTargets)
            {
                spriteBatch.Begin(effect: topLayer.BlurBelow ? _blurEffect : _layerEffects[renderTarget]);
                // Draw on Blur Render Target
                spriteBatch.Draw(renderTarget, _game.GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.End();
            }
            // Free GraphicsDevice
            _game.GraphicsDevice.SetRenderTarget(null);

            _renderTargets.Clear();
            _layerEffects.Clear();
            spriteBatch.Begin();
            spriteBatch.Draw(_blurRenderTarget, _game.GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.End();
        }

        spriteBatch.Begin(effect: topLayer.Effect);
        spriteBatch.Draw(topRenderTarget, _game.GraphicsDevice.Viewport.Bounds, Color.White);
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
        _blurRenderTarget.Dispose();
        foreach (Screen layer in _screenStack)
            layer.ApplyResolution(gameTime, uiScale);
        _blurRenderTarget = new(_game.GraphicsDevice, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height);
    }

    public bool ContainsLayer(Screen layer) => _screenStack.Contains(layer);
}