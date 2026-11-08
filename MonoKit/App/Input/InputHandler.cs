// AppHost - InputHandler.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoKit.App.Input;

internal static class EngineInputActions
{
    public static readonly byte ButtonPressed = byte.MaxValue;
    public static readonly byte SliderHold = byte.MaxValue - 1;
}

/// <summary>
///     Central input hub: registered <see cref="IInputDevice" />s write action flags each frame,
///     consumers read them via <see cref="IsActionActive" />/<see cref="HasAction" />.
///     Flags are cleared at the start of every <see cref="Update" />.
/// </summary>
public class InputHandler
{
    private readonly BitArray _actionsFlags = new(byte.MaxValue + 1);
    private readonly List<IInputDevice> _devices = new();

    private readonly object _textLock = new();
    private readonly Queue<char> _pendingCharacters = new();
    private readonly List<char> _frameCharacters = new();
    private bool _textSubscribed;

    public void RegisterDevice(IInputDevice device)
    {
        _devices.Add(device);
    }

    /// <summary>
    ///     Subscribes to the platform text-input event, feeding <see cref="TypedCharacters" /> each update.
    ///     Keyboard layout, modifiers and key repeat are handled by the OS.
    /// </summary>
    public void EnableTextInput(GameWindow window)
    {
        if (_textSubscribed)
            return;
        _textSubscribed = true;
        window.TextInput += OnTextInput;
    }

    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        lock (_textLock)
            _pendingCharacters.Enqueue(e.Character);
    }

    public void Update(double elapsedMilliseconds)
    {
        _actionsFlags.SetAll(false);
        _frameCharacters.Clear();
        lock (_textLock)
        {
            while (_pendingCharacters.Count > 0)
                _frameCharacters.Add(_pendingCharacters.Dequeue());
        }

        foreach (var device in _devices)
            device.Update(elapsedMilliseconds, _actionsFlags);
    }

    /// <summary>Characters typed since the last <see cref="Update" />, in input order.</summary>
    public IReadOnlyList<char> TypedCharacters => _frameCharacters;

    /// <summary>Returns whether the action fired this frame, without consuming it.</summary>
    public bool IsActionActive(byte actionID)
    {
        return _actionsFlags[actionID];
    }

    /// <summary>Consumes the action flag; returns true if it was set.</summary>
    private bool ConsumeAction(byte actionID)
    {
        var contains = _actionsFlags[actionID];
        _actionsFlags[actionID] = false;
        return contains;
    }

    /// <summary>Consumes the action; returns true if it fired this frame.</summary>
    public bool HasAction(byte actionID)
    {
        return ConsumeAction(actionID);
    }

    public void DoAction(byte actionID, Action function)
    {
        if (HasAction(actionID))
            function?.Invoke();
    }
}
