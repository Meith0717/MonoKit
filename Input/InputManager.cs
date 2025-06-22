// InputManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GameEngine.Input
{
    public class InputManager
    {
        private KeyboardListener _KeyboardListener = new();
        private MouseListener _MouseListener = new();

        public InputState Update(GameTime gameTime)
        {
            List<ActionType> actions = new List<ActionType>();

            _MouseListener.Listen(gameTime, ref actions, out Vector2 mousePosition);
            _KeyboardListener.Listener(ref actions, out string typedString);
            return new(actions, typedString, mousePosition);
        }
    }
}