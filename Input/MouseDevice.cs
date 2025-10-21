// MouseDevice.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;

namespace GameEngine.Input
{
    public enum MouseButtons { Left, Mid, Right, Forward, Back }

    public class MouseDevice : IInputDevice
    {
        private readonly Dictionary<(MouseButtons, InputEventType), byte> _bindings;
        private readonly Dictionary<MouseButtons, (ButtonState, ButtonState)> _buttonsStates = new();
        private MouseState _currentState, _previousState;

        public MouseDevice(Dictionary<(MouseButtons, InputEventType), byte> bindings)
        {
            _bindings = bindings;
        }

        public void Update(double elapsedMilliseconds, BitArray actionFlags)
        {
            _currentState = Mouse.GetState();
            _buttonsStates[MouseButtons.Left] = (_currentState.LeftButton, _previousState.LeftButton);
            _buttonsStates[MouseButtons.Mid] = (_currentState.MiddleButton, _previousState.MiddleButton);
            _buttonsStates[MouseButtons.Right] = (_currentState.RightButton, _previousState.RightButton);
            _buttonsStates[MouseButtons.Forward] = (_currentState.XButton1, _previousState.XButton1);
            _buttonsStates[MouseButtons.Back] = (_currentState.XButton2, _previousState.XButton2);
            _previousState = _currentState;

            foreach (var (button, (current, previous)) in _buttonsStates)
            {
                if (previous == ButtonState.Released && current == ButtonState.Released) break;

                var inputEventType = InputEventType.Released; // previous == ButtonState.Pressed && current == ButtonState.Released
                if (previous == ButtonState.Released && current == ButtonState.Pressed) inputEventType = InputEventType.Pressed;
                if (previous == ButtonState.Pressed && current == ButtonState.Pressed) inputEventType = InputEventType.Held;

                var inputEvent = (button, inputEventType);
                if (_bindings.TryGetValue(inputEvent, out var actionID))
                    actionFlags[actionID] = true;

                if (button == MouseButtons.Left && inputEventType == InputEventType.Released)
                    actionFlags[EngineInputActions.ButtonPressed] = true;

                if (button == MouseButtons.Left && inputEventType == InputEventType.Held)
                    actionFlags[EngineInputActions.SliderHold] = true;
            }
        }
    }
}