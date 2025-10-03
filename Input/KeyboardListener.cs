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
        private readonly Dictionary<int, ActionType> mActionOnMultiplePressed;
        private readonly Dictionary<Keys, ActionType> mActionOnPressed, mActionOnHold;
        private readonly Dictionary<Keys, KeyEventType> mKeysKeyEventTypes;
        private Keys[] mCurrentKeysPressed, mPreviousKeysPressed;

        public KeyboardListener()
        {
            mActionOnMultiplePressed = new()
            {
            };

            mActionOnPressed = new()
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

            mActionOnHold = new()
            {
                { Keys.W, ActionType.Accelerate },
                { Keys.S, ActionType.Break },
            };
            mKeysKeyEventTypes = new();
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
            mCurrentKeysPressed = keyboardState.GetPressedKeys();

            // Get KeyEventTypes (down or pressed) for keys.
            foreach (Keys key in mCurrentKeysPressed)
            {
                if (mPreviousKeysPressed == null)
                {
                    continue;
                }
                if (mPreviousKeysPressed.Contains(key))
                {
                    mKeysKeyEventTypes.Add(key, KeyEventType.OnButtonPressed);
                    continue;
                }
                mKeysKeyEventTypes.Add(key, KeyEventType.OnButtonDown);
            }
        }

        public void Listener(ref List<ActionType> actions)
        {
            mPreviousKeysPressed = mCurrentKeysPressed;
            mKeysKeyEventTypes.Clear();

            mCurrentKeysPressed = Keyboard.GetState().GetPressedKeys();
            UpdateKeysKeyEventTypes();

            if (mActionOnMultiplePressed.TryGetValue(Hash(mCurrentKeysPressed), out ActionType action))
                foreach (Keys key in mCurrentKeysPressed)
                    if (mKeysKeyEventTypes[key] == KeyEventType.OnButtonDown) actions.Add(action);

            foreach (Keys key in mCurrentKeysPressed)
            {
                if (mActionOnPressed.TryGetValue(key, out ActionType actionPressed))
                    if (mKeysKeyEventTypes[key] == KeyEventType.OnButtonDown) actions.Add(actionPressed);
                if (!mActionOnHold.TryGetValue(key, out ActionType actionHold)) continue;
                if (mKeysKeyEventTypes[key] == KeyEventType.OnButtonPressed) actions.Add(actionHold);
            }
        }
    }
}
