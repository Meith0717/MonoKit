// InputHandler.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoKit.Input
{
    internal static class EngineInputActions
    {
        public readonly static byte ButtonPressed = byte.MaxValue;
        public readonly static byte SliderHold = byte.MaxValue - 1;
    }

    public class InputHandler
    {
        private readonly List<IInputDevice> _devices = new();
        private readonly BitArray _actionsFlags = new(byte.MaxValue + 1);

        public void RegisterDevice(IInputDevice device)
            => _devices.Add(device);

        public void Update(double elapsedMilliseconds)
        {
            _actionsFlags.SetAll(false);
            foreach (var device in _devices)
                device.Update(elapsedMilliseconds, _actionsFlags);
        }

        public bool HasAction(byte actionID)
        {
            var contains = _actionsFlags[actionID];
            _actionsFlags[actionID] = false;
            return contains;
        }

        public void DoAction(byte actionID, Action function)
        {
            if (HasAction(actionID))
                function?.Invoke();
        }
    }
}
