// AppHost - PostProcessingRunner.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Rendering;

/// <summary>
///     Provides intermediate render targets for multi-pass post-processing effects.
///     Targets are created lazily at the current viewport size and recycled once per
///     frame via <see cref="BeginFrame" />, so a result returned by <see cref="Pass" />
///     stays valid until the next frame — regardless of pass count or nesting.
/// </summary>
public class PostProcessingRunner : IDisposable
{
    private readonly List<RenderTarget2D> _pool = [];
    private readonly List<RenderTarget2D> _rented = [];

    public void Dispose()
    {
        foreach (var target in _pool)
            target.Dispose();
        foreach (var target in _rented)
            target.Dispose();
        _pool.Clear();
        _rented.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>Returns all rented targets to the pool. Call once per frame before any effect runs.</summary>
    public void BeginFrame()
    {
        _pool.AddRange(_rented);
        _rented.Clear();
    }

    /// <summary>
    ///     Draws <paramref name="input" /> through <paramref name="effect" /> into a freshly
    ///     rented target. The input is NOT recycled, so intermediate results can be
    ///     referenced (e.g. as shader textures) later in the same frame.
    /// </summary>
    public RenderTarget2D Pass(
        Effect effect,
        SpriteBatch spriteBatch,
        GraphicsDevice graphicsDevice,
        RenderTarget2D input
    )
    {
        var destination = Rent(graphicsDevice);

        graphicsDevice.SetRenderTarget(destination);
        graphicsDevice.Clear(Color.Transparent);

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, effect);
        spriteBatch.Draw(input, input.Bounds, Color.White);
        spriteBatch.End();

        graphicsDevice.SetRenderTarget(null);

        return destination;
    }

    /// <summary>Drops all targets; they are recreated lazily at the new size on next use.</summary>
    public void ApplyResolution(int width, int height)
    {
        foreach (var target in _pool)
            target.Dispose();
        foreach (var target in _rented)
            target.Dispose();
        _pool.Clear();
        _rented.Clear();
    }

    private RenderTarget2D Rent(GraphicsDevice graphicsDevice)
    {
        var width = graphicsDevice.Viewport.Width;
        var height = graphicsDevice.Viewport.Height;

        while (_pool.Count > 0)
        {
            var target = _pool[^1];
            _pool.RemoveAt(_pool.Count - 1);
            if (target.Width == width && target.Height == height)
            {
                _rented.Add(target);
                return target;
            }

            target.Dispose();
        }

        var created = new RenderTarget2D(
            graphicsDevice,
            width,
            height,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );
        _rented.Add(created);
        return created;
    }
}
