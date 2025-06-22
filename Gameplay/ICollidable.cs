// ICollidable.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Engine.Runtime;
using Microsoft.Xna.Framework;

namespace Engine.Gameplay
{
    public interface ICollidable
    {
        float Mass { get; }

        void HasCollide(Vector2 position, GameRuntime scene);
    }
}
