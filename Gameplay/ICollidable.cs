// ICollidable.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Runtime;
using Microsoft.Xna.Framework;

namespace GameEngine.Gameplay
{
    public interface ICollidable
    {
        float Mass { get; }

        void HasCollide(Vector2 position, GameRuntime scene);
    }
}
