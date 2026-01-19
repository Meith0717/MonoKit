// GameRenderer2D.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Gameplay;
using MonoKit.Graphics.Camera;
using MonoKit.Spatial;

namespace MonoKit.Graphics.Rendering;

public class GameRenderer2D(RuntimeContainer services)
{
    private readonly SpatialHashing _spatialHashing = services.Get<SpatialHashing>();
    private readonly Camera2D _camera = services.Get<Camera2D>();
    private readonly List<IGameRendererProcess> _renderer2DProcesses = [];
    private readonly List<GameObject> _culledObjects = [];
    private float _viewportScale;
    private bool _hasBegan;

    public void AddProcess(IGameRendererProcess gameRendererProcess)
    {
        _renderer2DProcesses.Add(gameRendererProcess);
    }

    public void Update(double elapsedMilliseconds, float viewportScale)
    {
        _hasBegan = false;

        _culledObjects.Clear();
        _spatialHashing.GetInRectangle(_camera.Bounds, _culledObjects);
        foreach (var renderer2DProcess in _renderer2DProcesses)
            renderer2DProcess.UpdateEffects(elapsedMilliseconds);
        _viewportScale = viewportScale;
    }

    public void BeginDrawCameraTransformed(SpriteBatch spriteBatch)
    {
        _hasBegan = true;
        _camera.UpdateView(_viewportScale);
        spriteBatch.Begin(transformMatrix: _camera.View, sortMode: SpriteSortMode.BackToFront);
    }

    public void DrawTextures(SpriteBatch spriteBatch)
    {
        WasBeginCalled();
        foreach (var renderer2DProcess in _renderer2DProcesses)
            renderer2DProcess.DrawTextures(spriteBatch, services, _culledObjects);
    }

    public void DrawEffects(SpriteBatch spriteBatch)
    {
        WasBeginCalled();
        foreach (var renderer2DProcess in _renderer2DProcesses)
            renderer2DProcess.DrawEffects(spriteBatch, _camera.View, services, _culledObjects);
    }

    private void WasBeginCalled()
    {
        if (_hasBegan)
            return;
        throw new Exception("Call BeginDrawCameraTransformed");
    }
}
