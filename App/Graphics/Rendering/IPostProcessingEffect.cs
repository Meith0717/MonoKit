// IPostProcessingEffect.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Graphics.Rendering;

public interface IPostProcessingEffect
{
    RenderTarget2D Apply(
        SpriteBatch spriteBatch,
        PostProcessingRunner pipeline,
        RenderTarget2D input
    );
}

public class PostProcessingRunner(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly RenderTarget2D[] _renderTargets = new RenderTarget2D[2];
    private int _currentIndex;

    public void Dispose()
    {
        foreach (var renderTarget in _renderTargets)
            renderTarget?.Dispose();
        GC.SuppressFinalize(this);
    }

    public RenderTarget2D Pass(Effect effect, SpriteBatch spriteBatch, RenderTarget2D input)
    {
        var destination = GetNextRenderTarget();

        graphicsDevice.SetRenderTarget(destination);
        graphicsDevice.Clear(Color.Transparent);

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, effect);
        spriteBatch.Draw(input, input.Bounds, Color.White);
        spriteBatch.End();

        graphicsDevice.SetRenderTarget(null);

        return destination;
    }

    public void ApplyResolution(int width, int height)
    {
        for (var i = 0; i < _renderTargets.Length; i++)
        {
            _renderTargets[i]?.Dispose();
            _renderTargets[i] = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None
            );
        }
    }

    private RenderTarget2D GetNextRenderTarget()
    {
        _currentIndex = (_currentIndex + 1) % _renderTargets.Length;
        return _renderTargets[_currentIndex];
    }
}
