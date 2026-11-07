// MonoKit - MouseListener.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MonoKit.App.Input.Listeners;

public enum MouseButton
{
    Left,
    Mid,
    Right,
    Forward,
    Back,
    WheelForward,
    WheelBack,
}

public class MouseListener(Dictionary<(MouseButton, InputEventType), byte> bindings) : IInputDevice
{
    private const int HeldMaxCoolDown = 100; // Milliseconds

    private readonly Dictionary<MouseButton, double> _buttonHeldCoolDown = new()
    {
        { MouseButton.Left, HeldMaxCoolDown },
        { MouseButton.Mid, HeldMaxCoolDown },
        { MouseButton.Right, HeldMaxCoolDown },
        { MouseButton.Forward, HeldMaxCoolDown },
        { MouseButton.Back, HeldMaxCoolDown },
    };

    private readonly Dictionary<MouseButton, (ButtonState, ButtonState)> _buttonsStates = new();

    private MouseState _currentState,
        _previousState;

    public void Update(double elapsedMilliseconds, BitArray actionFlags)
    {
        _currentState = Mouse.GetState();
        _buttonsStates[MouseButton.Left] = (_currentState.LeftButton, _previousState.LeftButton);
        _buttonsStates[MouseButton.Mid] = (_currentState.MiddleButton, _previousState.MiddleButton);
        _buttonsStates[MouseButton.Right] = (_currentState.RightButton, _previousState.RightButton);
        _buttonsStates[MouseButton.Forward] = (_currentState.XButton1, _previousState.XButton1);
        _buttonsStates[MouseButton.Back] = (_currentState.XButton2, _previousState.XButton2);
        var wheelDelta = _currentState.ScrollWheelValue - _previousState.ScrollWheelValue;
        _previousState = _currentState;

        foreach (var (button, (current, previous)) in _buttonsStates)
        {
            InputEventType? inputEvent = null;

            if (previous == ButtonState.Released && current == ButtonState.Pressed)
            {
                inputEvent = InputEventType.Pressed;
            }
            else if (previous == ButtonState.Pressed && current == ButtonState.Released)
            {
                inputEvent = InputEventType.Released;
            }
            else if (previous == ButtonState.Pressed && current == ButtonState.Pressed)
            {
                _buttonHeldCoolDown[button] -= elapsedMilliseconds;
                if (_buttonHeldCoolDown[button] < 0)
                    inputEvent = InputEventType.Held;
            }
            else
            {
                _buttonHeldCoolDown[button] = HeldMaxCoolDown;
            }

            if (
                inputEvent.HasValue
                && bindings.TryGetValue((button, inputEvent.Value), out var actionID)
            )
                actionFlags[actionID] = true;

            if (button == MouseButton.Left && inputEvent == InputEventType.Released)
                actionFlags[EngineInputActions.ButtonPressed] = true;

            if (button == MouseButton.Left && inputEvent == InputEventType.Held)
                actionFlags[EngineInputActions.SliderHold] = true;
        }

        if (
            wheelDelta > 0
            && bindings.TryGetValue(
                (MouseButton.WheelForward, InputEventType.Pressed),
                out var wheelForwardID
            )
        )
            actionFlags[wheelForwardID] = true;
        else if (
            wheelDelta < 0
            && bindings.TryGetValue(
                (MouseButton.WheelBack, InputEventType.Pressed),
                out var wheelBackID
            )
        )
            actionFlags[wheelBackID] = true;
    }
}
