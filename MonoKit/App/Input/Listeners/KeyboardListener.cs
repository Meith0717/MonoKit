// MonoKit - KeyboardListener.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace MonoKit.App.Input.Listeners;

public class KeyboardListener(Dictionary<(Keys, InputEventType), byte> bindings) : IInputDevice
{
    private readonly List<(Keys, InputEventType)> _pressedKeyStates = new();

    private Keys[] _currentPressedKeys,
        _previousPressedKeys = [];

    /// <summary>Detects key transitions and sets bound action flags.</summary>
    public void Update(double elapsedMilliseconds, BitArray actionFlags)
    {
        _pressedKeyStates.Clear();

        _currentPressedKeys = Keyboard.GetState().GetPressedKeys();
        var currentSet = _currentPressedKeys.ToHashSet();
        var previousSet = _previousPressedKeys.ToHashSet();

        foreach (var key in currentSet)
            if (previousSet.Contains(key))
                _pressedKeyStates.Add((key, InputEventType.Held));
            else
                _pressedKeyStates.Add((key, InputEventType.Pressed));

        foreach (var key in previousSet)
            if (!currentSet.Contains(key))
                _pressedKeyStates.Add((key, InputEventType.Released));

        _previousPressedKeys = _currentPressedKeys.ToArray();

        foreach (var item in _pressedKeyStates)
            if (bindings.TryGetValue(item, out var actionID))
                actionFlags[actionID] = true;
    }
}
