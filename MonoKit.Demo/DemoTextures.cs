// DemoTextures.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App;

namespace MonoKit.Demo;

/// <summary>
///     Generates stand-in textures for UI art that has not been provided yet (arrows, toggles).
///     Once the real PNGs exist in Content, they win: the generated ones are only added when
///     the content id is missing.
/// </summary>
public static class DemoTextures
{
    private static readonly Color Accent = new(240, 130, 30, 255);

    /// <summary>Sprite scale factor that renders a texture at the target width in pixels.</summary>
    public static float ScaleFor(Texture2D texture, float targetWidth) => targetWidth / texture.Width;

    public static void EnsureUiElements(AppHost appHost, GraphicsDevice graphicsDevice)
    {
        var container = appHost.ContentProvider.Container<Texture2D>();
        if (container.GetId("ui_arrow_l") < 0)
            container.Add("ui_arrow_l", Arrow(graphicsDevice, left: true));
        if (container.GetId("ui_arrow_r") < 0)
            container.Add("ui_arrow_r", Arrow(graphicsDevice, left: false));
        if (container.GetId("ui_toggle_true") < 0)
            container.Add("ui_toggle_true", Toggle(graphicsDevice, @checked: true));
        if (container.GetId("ui_toggle_false") < 0)
            container.Add("ui_toggle_false", Toggle(graphicsDevice, @checked: false));
    }

    private static Texture2D Create(GraphicsDevice gd, int w, int h, Func<int, int, Color> pixel)
    {
        var data = new Color[w * h];
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
                data[y * w + x] = pixel(x, y);

        var texture = new Texture2D(gd, w, h);
        texture.SetData(data);
        return texture;
    }

    private static Texture2D Arrow(GraphicsDevice gd, bool left)
    {
        const int size = 28;
        var tipX = left ? 0.2f : 0.8f;
        var baseX = left ? 0.8f : 0.2f;

        return Create(gd, size, size, (x, y) =>
        {
            var cx = x / ( float)size;
            var cy = y / ( float)size;
            var t = MathF.Abs(cx - tipX) / MathF.Abs(baseX - tipX);
            var inside = t <= 1f && MathF.Abs(cy - 0.5f) <= t * 0.3f + 0.02f;
            return inside ? Accent : Color.Transparent;
        });
    }

    private static Texture2D Toggle(GraphicsDevice gd, bool @checked)
    {
        const int size = 28;

        return Create(gd, size, size, (x, y) =>
        {
            var edge = Math.Min(Math.Min(x, size - 1 - x), Math.Min(y, size - 1 - y));
            if (edge < 2)
                return Color.Gray;
            if (!@checked)
                return Color.Transparent;
            return edge >= 6 ? new Color(230, 230, 230, 255) : Color.Transparent;
        });
    }
}
