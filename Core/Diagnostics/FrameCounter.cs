// FrameCounter.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.Core.Diagnostics;

public class FrameCounter(SpriteFont spriteFont)
{
    private int _samples;
    private double _summedFps;

    private double _timeSinceLastSample;

    public long TotalFrames { get; private set; }
    public double TotalSeconds { get; private set; }
    public double AverageFramesPerSecond { get; private set; }
    public double MinFramesPerSecond { get; private set; } = float.MaxValue;
    public double MaxFramesPerSecond { get; private set; }
    public double CurrentFramesPerSecond { get; private set; }
    public double FrameTime { get; private set; }

    public void Update(double elapsedSeconds, double elapsedMilliseconds)
    {
        TotalFrames++;
        _timeSinceLastSample += elapsedSeconds;
        TotalSeconds += elapsedSeconds;

        if (_timeSinceLastSample < 1.0)
            return;

        FrameTime = elapsedMilliseconds;
        CurrentFramesPerSecond = 1d / FrameTime * 1e3d;

        _samples++;
        _summedFps += CurrentFramesPerSecond;
        MinFramesPerSecond = double.Min(CurrentFramesPerSecond, MinFramesPerSecond);
        MaxFramesPerSecond = double.Max(CurrentFramesPerSecond, MaxFramesPerSecond);
        AverageFramesPerSecond = _summedFps / _samples;

        _timeSinceLastSample = 0;
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport, float viewpointScale)
    {
        var scale = .12f;
        var str = double.Round(CurrentFramesPerSecond).ToString();
        var strSize = spriteFont.MeasureString(str) * scale * viewpointScale;
        var ofset = new Vector2(10, -10) * scale * viewpointScale;
        var position = new Vector2(viewport.Width - strSize.X, 0) - ofset;
        spriteBatch.DrawString(
            spriteFont,
            str,
            position,
            Color.White,
            0,
            Vector2.Zero,
            scale * viewpointScale,
            SpriteEffects.None,
            1
        );
    }
}
