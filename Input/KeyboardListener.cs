// KeyboardListener.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Input
{
    public class KeyboardListener
    {
        private readonly Dictionary<int, ActionType> _actionOnMultiplePressed;
        private readonly Dictionary<Keys, ActionType> _actionOnPressed, _actionOnHold;
        private readonly Dictionary<Keys, KeyEventType> _keysKeyEventTypes;
        private Keys[] _currentKeysPressed, _previousKeysPressed;

        public KeyboardListener()
        {
            _actionOnMultiplePressed = new()
            {
            };

            _actionOnPressed = new()
            {
                { Keys.Escape, ActionType.ESC },
                { Keys.Space, ActionType.Space },
                { Keys.LeftShift, ActionType.Boost },
                { Keys.Q, ActionType.SwitchWeapon },
                { Keys.E, ActionType.ObjectInteraction },
                { Keys.I, ActionType.OpenCargo },
                { Keys.M, ActionType.OpenMap },
                { Keys.Up, ActionType.ScrollUp },
                { Keys.Down, ActionType.ScrollDown },
                { Keys.R, ActionType.ReloadUi }
            };

            _actionOnHold = new()
            {
                { Keys.W, ActionType.Accelerate },
                { Keys.S, ActionType.Break },
            };
            _keysKeyEventTypes = new();
        }

        private static int Hash(params Keys[] keys)
        {
            int tmp = 0;
            Array.Sort(keys);
            for (int i = keys.Length - 1; i >= 0; i--)
            {
                Keys key = keys[i];
                tmp += (int)key * (int)Math.Pow(1000, i);
            }
            return tmp;
        }

        private void UpdateKeysKeyEventTypes()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            _currentKeysPressed = keyboardState.GetPressedKeys();

            // Get KeyEventTypes (down or pressed) for keys.
            foreach (Keys key in _currentKeysPressed)
            {
                if (_previousKeysPressed == null)
                {
                    continue;
                }
                if (_previousKeysPressed.Contains(key))
                {
                    _keysKeyEventTypes.Add(key, KeyEventType.OnButtonPressed);
                    continue;
                }
                _keysKeyEventTypes.Add(key, KeyEventType.OnButtonDown);
            }
        }

        public void Listener(ref List<ActionType> actions)
        {
            _previousKeysPressed = _currentKeysPressed;
            _keysKeyEventTypes.Clear();

            _currentKeysPressed = Keyboard.GetState().GetPressedKeys();
            UpdateKeysKeyEventTypes();

            if (_actionOnMultiplePressed.TryGetValue(Hash(_currentKeysPressed), out ActionType action))
                foreach (Keys key in _currentKeysPressed)
                    if (_keysKeyEventTypes[key] == KeyEventType.OnButtonDown) actions.Add(action);

            foreach (Keys key in _currentKeysPressed)
            {
                if (_actionOnPressed.TryGetValue(key, out ActionType actionPressed))
                    if (_keysKeyEventTypes[key] == KeyEventType.OnButtonDown) actions.Add(actionPressed);
                if (!_actionOnHold.TryGetValue(key, out ActionType actionHold)) continue;
                if (_keysKeyEventTypes[key] == KeyEventType.OnButtonPressed) actions.Add(actionHold);
            }
        }
    }
}
