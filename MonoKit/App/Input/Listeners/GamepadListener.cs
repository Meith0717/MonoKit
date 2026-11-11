// MonoKit - GamepadListener.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoKit.App.Input.Listeners;

/// <summary>
///     Maps gamepad button events of one player to action IDs.
///     Emits Pressed on the press transition, Held while held, Released on release.
/// </summary>
public class GamepadListener(
    Dictionary<(Buttons, InputEventType), byte> bindings,
    PlayerIndex playerIndex = PlayerIndex.One
) : IInputDevice
{
    private HashSet<Buttons> _previousButtons = [];

    /// <summary>Compares current and previous buttons and sets bound action flags.</summary>
    public void Update(double elapsedMilliseconds, BitArray actionFlags)
    {
        var current = GetPressedButtons(GamePad.GetState(playerIndex));

        foreach (var button in current)
        {
            var type = _previousButtons.Contains(button)
                ? InputEventType.Held
                : InputEventType.Pressed;
            if (bindings.TryGetValue((button, type), out var actionID))
                actionFlags[actionID] = true;
        }

        foreach (var button in _previousButtons)
            if (
                !current.Contains(button)
                && bindings.TryGetValue((button, InputEventType.Released), out var actionID)
            )
                actionFlags[actionID] = true;

        _previousButtons = current;
    }

    /// <summary>Collects all currently pressed buttons of a connected gamepad.</summary>
    private static HashSet<Buttons> GetPressedButtons(GamePadState state)
    {
        var pressed = new HashSet<Buttons>();
        if (!state.IsConnected)
            return pressed;

        if (state.Buttons.A == ButtonState.Pressed)
            pressed.Add(Buttons.A);
        if (state.Buttons.B == ButtonState.Pressed)
            pressed.Add(Buttons.B);
        if (state.Buttons.X == ButtonState.Pressed)
            pressed.Add(Buttons.X);
        if (state.Buttons.Y == ButtonState.Pressed)
            pressed.Add(Buttons.Y);
        if (state.Buttons.Start == ButtonState.Pressed)
            pressed.Add(Buttons.Start);
        if (state.Buttons.Back == ButtonState.Pressed)
            pressed.Add(Buttons.Back);
        if (state.Buttons.LeftShoulder == ButtonState.Pressed)
            pressed.Add(Buttons.LeftShoulder);
        if (state.Buttons.RightShoulder == ButtonState.Pressed)
            pressed.Add(Buttons.RightShoulder);
        if (state.Buttons.LeftStick == ButtonState.Pressed)
            pressed.Add(Buttons.LeftStick);
        if (state.Buttons.RightStick == ButtonState.Pressed)
            pressed.Add(Buttons.RightStick);
        if (state.DPad.Up == ButtonState.Pressed)
            pressed.Add(Buttons.DPadUp);
        if (state.DPad.Down == ButtonState.Pressed)
            pressed.Add(Buttons.DPadDown);
        if (state.DPad.Left == ButtonState.Pressed)
            pressed.Add(Buttons.DPadLeft);
        if (state.DPad.Right == ButtonState.Pressed)
            pressed.Add(Buttons.DPadRight);

        return pressed;
    }
}
