// InputHandler.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoKit.Input;

internal static class EngineInputActions
{
    public static readonly byte ButtonPressed = byte.MaxValue;
    public static readonly byte SliderHold = byte.MaxValue - 1;
}

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
