// MouseListener.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;

namespace MonoKit.Input
{
    public enum MouseButton { Left, Mid, Right, Forward, Back, WheelForward, WheelBack }

    public class MouseListener(Dictionary<(MouseButton, InputEventType), byte> bindings) : IInputDevice
    {
        private const int HeldMaxCoolDown = 100; // Milliseconds
        private readonly Dictionary<(MouseButton, InputEventType), byte> _bindings = bindings;
        private readonly Dictionary<MouseButton, (ButtonState, ButtonState)> _buttonsStates = new();
        private MouseState _currentState, _previousState;
        private Dictionary<MouseButton, double> _buttonHeldCoolDown = new(){
            { MouseButton.Left, HeldMaxCoolDown },
            { MouseButton.Mid, HeldMaxCoolDown },
            { MouseButton.Right, HeldMaxCoolDown },
            { MouseButton.Forward, HeldMaxCoolDown },
            { MouseButton.Back, HeldMaxCoolDown }
        };

        public void Update(double elapsedMilliseconds, BitArray actionFlags)
        {
            _currentState = Mouse.GetState();
            _buttonsStates[MouseButton.Left] = (_currentState.LeftButton, _previousState.LeftButton);
            _buttonsStates[MouseButton.Mid] = (_currentState.MiddleButton, _previousState.MiddleButton);
            _buttonsStates[MouseButton.Right] = (_currentState.RightButton, _previousState.RightButton);
            _buttonsStates[MouseButton.Forward] = (_currentState.XButton1, _previousState.XButton1);
            _buttonsStates[MouseButton.Back] = (_currentState.XButton2, _previousState.XButton2);
            _previousState = _currentState;

            foreach (var (button, (current, previous)) in _buttonsStates)
            {
                if (previous == ButtonState.Released && current == ButtonState.Released)
                {
                    _buttonHeldCoolDown[button] = HeldMaxCoolDown;
                    continue;
                }

                var inputEventType = InputEventType.Pressed;

                if (previous == ButtonState.Pressed && current == ButtonState.Released)
                    inputEventType = InputEventType.Released;

                if (previous == ButtonState.Pressed && current == ButtonState.Pressed)
                {
                    _buttonHeldCoolDown[button] -= elapsedMilliseconds;
                    if (_buttonHeldCoolDown[button] < 0)
                        inputEventType = InputEventType.Held;
                }

                var inputEvent = (button, inputEventType);
                if (_bindings.TryGetValue(inputEvent, out var actionID))
                    actionFlags[actionID] = true;

                if (button == MouseButton.Left && inputEventType == InputEventType.Released)
                    actionFlags[EngineInputActions.ButtonPressed] = true;

                if (button == MouseButton.Left && inputEventType == InputEventType.Held)
                    actionFlags[EngineInputActions.SliderHold] = true;
            }
        }
    }
}