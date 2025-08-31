// PostProcessingTool.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Rendering
{
    public interface IPostProcessingEffect
    {
        public RenderTarget2D Apply(SpriteBatch spriteBatch, PostProcessingRunner pipeline, RenderTarget2D input);
    }
}
