// AppHost - FrameCounter.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Core.Diagnostics;

public class FrameCounter
{
    private int _samples;
    private double _summedFps;
    private int _windowFrames;

    private double _timeSinceLastSample;

    public long TotalFrames { get; private set; }
    public double TotalSeconds { get; private set; }
    public double AverageFramesPerSecond { get; private set; }
    public double MinFramesPerSecond { get; private set; } = float.MaxValue;
    public double MaxFramesPerSecond { get; private set; }
    public double CurrentFramesPerSecond { get; private set; }
    public double FrameTime { get; private set; }

    /// <summary>Accumulates frame samples and refreshes FPS statistics each second.</summary>
    public void Update(double elapsedSeconds, double elapsedMilliseconds)
    {
        TotalFrames++;
        _windowFrames++;
        _timeSinceLastSample += elapsedSeconds;
        TotalSeconds += elapsedSeconds;

        if (_timeSinceLastSample < 1.0)
            return;

        FrameTime = elapsedMilliseconds;
        CurrentFramesPerSecond = _windowFrames / _timeSinceLastSample;
        _windowFrames = 0;

        _samples++;
        _summedFps += CurrentFramesPerSecond;
        MinFramesPerSecond = double.Min(CurrentFramesPerSecond, MinFramesPerSecond);
        MaxFramesPerSecond = double.Max(CurrentFramesPerSecond, MaxFramesPerSecond);
        AverageFramesPerSecond = _summedFps / _samples;

        _timeSinceLastSample = 0;
    }
}
