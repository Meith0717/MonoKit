// IInputDevice.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections;

namespace MonoKit.Input;

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
