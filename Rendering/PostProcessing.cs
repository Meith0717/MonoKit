// PostProcessing.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngine.Rendering
{
    public delegate RenderTarget2D PostProcessingDelegate(SpriteBatch spriteBatch, PostProcessing postProcessing, RenderTarget2D input);

    public class PostProcessing(GraphicsDevice graphicsDevice) : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
        private readonly RenderTarget2D[] _renderTargets = new RenderTarget2D[2];
        private int currentIndex = 0;

        public void ApplyResolution(int width, int height)
        {
            for (var i = 0; i < _renderTargets.Length; i++)
            {
                _renderTargets[i]?.Dispose();
                _renderTargets[i] = new RenderTarget2D(
                _graphicsDevice,
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
            currentIndex = (currentIndex + 1) % _renderTargets.Length;
            return _renderTargets[currentIndex];
        }

        public RenderTarget2D Apply(SpriteBatch spriteBatch, RenderTarget2D input, Effect effect)
        {
            var destination = GetNextRenderTarget();

            _graphicsDevice.SetRenderTarget(destination);
            _graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, effect);
            spriteBatch.Draw(input, input.Bounds, Color.White);
            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(null);

            return destination;
        }

        public void Dispose()
        {
            foreach (var renderTarget in _renderTargets)
                renderTarget?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
