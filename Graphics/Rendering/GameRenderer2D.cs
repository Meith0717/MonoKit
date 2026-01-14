// GameRenderer2D.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Core.Extensions;
using MonoKit.Gameplay;
using MonoKit.Graphics.Camera;
using MonoKit.Spatial;
#if DEBUG
using MonoGame.Extended;
# endif

namespace MonoKit.Graphics.Rendering;

public class GameRenderer2D(RuntimeContainer services)
{
    private readonly SpatialHashing _spatialHashing = services.Get<SpatialHashing>();
    private readonly Camera2D _camera = services.Get<Camera2D>();
    private readonly List<IRenderer2DProcess> _renderer2DProcesses = [];
    private readonly List<GameObject> _culledObjects = [];
    private float _viewportScale;

    public void AddProcess(IRenderer2DProcess renderer2DProcess)
    {
        _renderer2DProcesses.Add(renderer2DProcess);
    }

    public void Update(double elapsedMilliseconds, float viewportScale)
    {
        _culledObjects.Clear();
        _spatialHashing.GetInRectangle(_camera.Bounds, _culledObjects);
        foreach (var renderer2DProcess in _renderer2DProcesses)
            renderer2DProcess.UpdateEffects(elapsedMilliseconds);
        _viewportScale = viewportScale;
    }

    public void Begin(SpriteBatch spriteBatch)
    {
        _camera.UpdateView(_viewportScale);
        spriteBatch.Begin(transformMatrix: _camera.View, sortMode: SpriteSortMode.BackToFront);
    }

    public void DrawTextures(SpriteBatch spriteBatch)
    {
        foreach (var renderer2DProcess in _renderer2DProcesses)
            renderer2DProcess.DrawTextures(spriteBatch, services, _culledObjects);
    }

    public void DrawEffects(SpriteBatch spriteBatch)
    {
        foreach (var renderer2DProcess in _renderer2DProcesses)
            renderer2DProcess.DrawEffects(spriteBatch, _camera.View, services, _culledObjects);
    }
}
