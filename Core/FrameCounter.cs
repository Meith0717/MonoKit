// FrameCounter.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Core
{
    public class FrameCounter()
    {
        private int _samples;
        private double _summedFps;

        public long TotalFrames { get; private set; }
        public double TotalSeconds { get; private set; }
        public double AverageFramesPerSecond { get; private set; }
        public double MinFramesPerSecond { get; private set; } = float.MaxValue;
        public double MaxFramesPerSecond { get; private set; }
        public double CurrentFramesPerSecond { get; private set; }
        public double FrameTime { get; private set; }

        private double _timeSinceLastSample;

        private GameTime _gameTime;

        public void SetGameTime(GameTime gameTime)
            => _gameTime = gameTime;

        public void Update()
        {
            TotalFrames++;
            _timeSinceLastSample += _gameTime.ElapsedGameTime.TotalSeconds;
            TotalSeconds += _gameTime.ElapsedGameTime.TotalSeconds;

            if (_timeSinceLastSample < 1.0)
                return;

            FrameTime = _gameTime.ElapsedGameTime.TotalMilliseconds;
            CurrentFramesPerSecond = 1d / FrameTime * 1e3d;

            _samples++;
            _summedFps += CurrentFramesPerSecond;
            MinFramesPerSecond = double.Min(CurrentFramesPerSecond, MinFramesPerSecond);
            MaxFramesPerSecond = double.Max(CurrentFramesPerSecond, MaxFramesPerSecond);
            AverageFramesPerSecond = _summedFps / _samples;

            _timeSinceLastSample = 0;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString("defaultFont", $"{double.Round(CurrentFramesPerSecond)} fps", new Vector2(1, 1), Color.White, .12f);
        }
    }
}
