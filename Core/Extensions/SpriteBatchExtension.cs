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
        var texture = ContentProvider.Container<Texture2D>().Get(gameObject.TextureId);
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

    public static void DrawString(
        this SpriteBatch spriteBatch,
        string font,
        string text,
        Vector2 position,
        Color color,
        float scale
    )
    {
        var spriteFont = ContentProvider.Container<SpriteFont>().Get(font);
        spriteBatch.DrawString(
            spriteFont,
            text,
            position,
            color,
            0,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            1
        );
    }

    public static void Draw(
        this SpriteBatch spriteBatch,
        string id,
        Vector2 position,
        float scale,
        float rotation,
        int depth,
        Color color
    )
    {
        var texture = ContentProvider.Container<Texture2D>().Get(id);
        Vector2 offset = new(texture.Width / 2, texture.Height / 2);
        spriteBatch.Draw(
            texture,
            position,
            null,
            color,
            rotation,
            offset,
            scale,
            SpriteEffects.None,
            GetDepth(depth)
        );
    }

    public static void Draw(
        this SpriteBatch spriteBatch,
        string id,
        Vector2 position,
        float width,
        float height,
        Color color
    )
    {
        var texture = ContentProvider.Container<Texture2D>().Get(id);
        spriteBatch.Draw(
            texture,
            new RectangleF(position.X, position.Y, width, height).ToRectangle(),
            null,
            color,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );
    }

    public static void Draw(
        this SpriteBatch spriteBatch,
        string id,
        Vector2 position,
        float width,
        float height,
        Vector2 offset,
        float rotation
    )
    {
        var texture = ContentProvider.Container<Texture2D>().Get(id);
        spriteBatch.Draw(
            texture,
            new RectangleF(
                position.X + offset.X,
                position.Y + offset.Y,
                width,
                height
            ).ToRectangle(),
            null,
            Color.White,
            rotation,
            new Vector2(texture.Width / 2, texture.Height / 2),
            SpriteEffects.None,
            1
        );
    }
}
