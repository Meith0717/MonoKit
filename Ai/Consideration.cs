// Consideration.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace MonoKit.Ai
{
    public abstract class Consideration
    {
        public abstract float Score();

        protected float Clamp01(float value) => MathHelper.Clamp(value, 0f, 1f);
    }
}
