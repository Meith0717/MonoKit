// MonoKit - IInputDevice.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections;

namespace MonoKit.App.Input;

public enum InputEventType
{
    Pressed,
    Released,
    Held,
}

public interface IInputDevice
{
    void Update(double elapsedMilliseconds, BitArray actionFlags);
}
