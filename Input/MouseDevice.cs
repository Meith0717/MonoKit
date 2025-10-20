// MouseDevice.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GameEngine.Input
{
    public enum MouseButtons { Left, Mid, Right, Forward, Back }

    public class MouseDevice<TActionType>(Dictionary<(MouseButtons, InputEventType), TActionType> bindings) : IInputDevice<TActionType>
    {
        private readonly Dictionary<(MouseButtons, InputEventType), TActionType> _bindings = bindings;
        private readonly Dictionary<MouseButtons, (ButtonState, ButtonState)> _buttonsStates = new();
        private readonly Dictionary<MouseButtons, (ButtonState, ButtonState)> _previousButtonState = new();
        private MouseState _currentState, _previousState;

        public void Update(double elapsedMilliseconds, List<TActionType> actions)
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
                if (_bindings.TryGetValue(inputEvent, out var action))
                    actions.Add(action);
            }
        }
    }
}