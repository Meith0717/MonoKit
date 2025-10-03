// InputManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GameEngine.Input
{
    public class InputManager
    {
        private readonly KeyboardListener _keyboardListener = new();
        private readonly MouseListener _mouseListener = new();
        private readonly InputState _inputState = new();
        private List<ActionType> _actions = new();

        public InputState Update(GameTime gameTime)
        {
            _mouseListener.Listen(gameTime, ref _actions, out Vector2 mousePosition);
            _keyboardListener.Listener(ref _actions);

            _inputState.UpdateData(_actions, mousePosition);
            _actions.Clear();

            return _inputState;
        }
    }
}