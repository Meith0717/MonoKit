// CameraShaker.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace Engine.Camera
{
    public class CameraShaker
    {
        private Vector2 shakeOffset = Vector2.Zero;
        private float shakeStrength = 0f;
        private float shakeDuration = 0f;
        private float shakeTimer = 0f;

        public void Shake(float strength, float duration)
        {
            shakeStrength = strength;
            shakeDuration = duration;
            shakeTimer = duration;
        }

        public void Update(Camera2d camera, GameTime gameTime)
        {
            if (shakeTimer > 0)
            {
                Random random = new Random();
                float percentComplete = 1 - shakeTimer / shakeDuration;
                float damping = 1f - MathHelper.Clamp(2 * percentComplete - 1, 0, 1);

                camera.Position += new Vector2(
                    MathHelper.Lerp(-shakeStrength, shakeStrength, (float)random.NextDouble()) * damping,
                    MathHelper.Lerp(-shakeStrength, shakeStrength, (float)random.NextDouble()) * damping
                );

                shakeTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                shakeOffset = Vector2.Zero;
            }
        }
    }
}
