// IPostProcessingEffect.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Rendering;

/// <summary>
/// A post-processing effect applied to a rendered screen. Implementations chain
/// passes through <see cref="PostProcessingRunner.Pass"/>; the returned target is
/// owned by the runner and stays valid until the runner's next
/// <see cref="PostProcessingRunner.BeginFrame"/> call.
/// </summary>
public interface IPostProcessingEffect
{
    /// <param name="spriteBatch">Sprite batch used for the fullscreen pass draws.</param>
    /// <param name="pipeline">Runner providing intermediate render targets.</param>
    /// <param name="input">The rendered input to process.</param>
    /// <returns>The processed image, as a runner-owned render target.</returns>
    RenderTarget2D Apply(
        SpriteBatch spriteBatch,
        PostProcessingRunner pipeline,
        RenderTarget2D input
    );
}
