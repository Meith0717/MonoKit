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
                { Keys.I, ActionType.OpenCargo },
                { Keys.O, ActionType.OpenShop },
                { Keys.M, ActionType.OpenMap },
            };

            mActionOnHold = new()
            {
                { Keys.W, ActionType.Accelerate },
                { Keys.S, ActionType.Break },
            };
            mKeysKeyEventTypes = new();
        }

        private int Hash(params Keys[] keys)
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

        public void Listener(ref List<ActionType> actions, out string typedOutput)
        {
            typedOutput = "";

            mPreviousKeysPressed = mCurrentKeysPressed;
            mKeysKeyEventTypes.Clear();

            mCurrentKeysPressed = Keyboard.GetState().GetPressedKeys();
            UpdateKeysKeyEventTypes();

            foreach (Keys pressedKey in mCurrentKeysPressed)
            {
                typedOutput += GetPressedAlphabet(pressedKey);
                typedOutput += GetPressedDigs(pressedKey);
                if (pressedKey == Keys.LeftShift || pressedKey == Keys.RightShift || Keyboard.GetState().CapsLock)
                    typedOutput = typedOutput.ToUpper();
            }

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

        private string GetPressedAlphabet(Keys pressedKey)
        {
            if (mPreviousKeysPressed.Contains(pressedKey)) return "";
            if (pressedKey == Keys.Space) return " ";
            if (pressedKey >= Keys.A && pressedKey <= Keys.Z)
                return ((char)('a' + (pressedKey - Keys.A))).ToString();
            return "";
        }

        private string GetPressedDigs(Keys pressedKey)
        {
            if (mPreviousKeysPressed.Contains(pressedKey)) return "";
            if (pressedKey >= Keys.D0 && pressedKey <= Keys.D9)
                return ((char)('0' + (pressedKey - Keys.D0))).ToString();
            return "";
        }
    }
}
