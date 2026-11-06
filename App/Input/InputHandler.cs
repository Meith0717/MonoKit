// InputHandler.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoKit.App.Input;

internal static class EngineInputActions
{
    public static readonly byte ButtonPressed = byte.MaxValue;
    public static readonly byte SliderHold = byte.MaxValue - 1;
}

/// <summary>
/// Central input hub: registered <see cref="IInputDevice"/>s write action flags each frame,
/// consumers read them via <see cref="IsActionActive"/>/<see cref="HasAction"/>.
/// Flags are cleared at the start of every <see cref="Update"/>.
/// </summary>
public class InputHandler
{
    private readonly BitArray _actionsFlags = new(byte.MaxValue + 1);
    private readonly List<IInputDevice> _devices = new();

    public void RegisterDevice(IInputDevice device)
    {
        _devices.Add(device);
    }

    public void Update(double elapsedMilliseconds)
    {
        _actionsFlags.SetAll(false);
        foreach (var device in _devices)
            device.Update(elapsedMilliseconds, _actionsFlags);
    }

    /// <summary>Returns whether the action fired this frame, without consuming it.</summary>
    public bool IsActionActive(byte actionID) => _actionsFlags[actionID];

    /// <summary>Consumes the action flag; returns true if it was set.</summary>
    public bool ConsumeAction(byte actionID)
    {
        var contains = _actionsFlags[actionID];
        _actionsFlags[actionID] = false;
        return contains;
    }

    /// <summary>Consumes the action; returns true if it fired this frame.</summary>
    public bool HasAction(byte actionID) => ConsumeAction(actionID);

    public void DoAction(byte actionID, Action function)
    {
        if (HasAction(actionID))
            function?.Invoke();
    }
}
