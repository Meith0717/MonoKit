// PostProcessingTool.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Rendering
{
    public delegate RenderTarget2D PostProcessingDelegate(SpriteBatch spriteBatch, PostProcessing postProcessing, RenderTarget2D input);
}
