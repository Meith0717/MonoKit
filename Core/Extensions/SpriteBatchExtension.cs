// SpriteBatchExtension.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Content;
using MonoKit.Gameplay;

namespace MonoKit.Core.Extensions;

public static class SpriteBatchExtension
{
    public const float MaxLayerDepth = 1000;

    private static float GetDepth(int depth)
    {
        var fDepth = depth / MaxLayerDepth;
        if (fDepth > 1)
            throw new Exception();
        return fDepth;
    }

    public static void DrawGameObject(this SpriteBatch spriteBatch, GameObject gameObject)
    {
        var texture = gameObject.Texture;
        var position = gameObject.Position;
        var rotation = gameObject.MovingDirection.InclinationAngle();
        var scale = gameObject.Scale;
        var drawHeight = GetDepth(gameObject.RenderingDepth);
        var offset = Vector2.Divide(new Vector2(texture.Width, texture.Height), 2);

        spriteBatch.Draw(
            texture,
            position,
            null,
            gameObject.TextureColor,
            rotation,
            offset,
            scale,
            SpriteEffects.None,
            drawHeight
        );
    }

    public static void DrawCircleF(
        this SpriteBatch spriteBatch,
        Vector2 center,
        float radius,
        Color color,
        float thicknessScale,
        int depth = 1
    )
    {
        spriteBatch.DrawCircle(center, radius, 25, color, 1 / thicknessScale, GetDepth(depth));
    }

    public static void DrawRectangleF(
        this SpriteBatch spriteBatch,
        RectangleF rectangle,
        Color color,
        float thicknessScale = 1,
        int depth = 1
    )
    {
        spriteBatch.DrawRectangle(rectangle, color, 1 / thicknessScale, GetDepth(depth));
    }
}
