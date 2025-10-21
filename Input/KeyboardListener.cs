// KeyboardDevice.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.Input
{
    public class KeyboardListener(Dictionary<(Keys, InputEventType), byte> bindings)
        : IInputDevice
    {
        private readonly Dictionary<(Keys, InputEventType), byte> _bindings = bindings;
        private Keys[] _currentPressedKeys, _previousPressedKeys = [];
        private readonly List<(Keys, InputEventType)> _pressedKeyStates = new();

        public void Update(double elapsedMilliseconds, BitArray actionFlags)
        {
            _pressedKeyStates.Clear();

            _currentPressedKeys = Keyboard.GetState().GetPressedKeys();
            var currentSet = _currentPressedKeys.ToHashSet();
            var previousSet = _previousPressedKeys.ToHashSet();

            foreach (var key in currentSet)
            {
                if (previousSet.Contains(key))
                    _pressedKeyStates.Add((key, InputEventType.Held));
                else
                    _pressedKeyStates.Add((key, InputEventType.Pressed));
            }

            foreach (var key in previousSet)
            {
                if (!currentSet.Contains(key))
                    _pressedKeyStates.Add((key, InputEventType.Released));
            }

            _previousPressedKeys = _currentPressedKeys.ToArray();

            foreach (var item in _pressedKeyStates)
            {
                if (_bindings.TryGetValue(item, out var actionID))
                    actionFlags[actionID] = true;
            }
        }
    }
}
