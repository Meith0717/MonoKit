// IInputDevice.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections;

namespace MonoKit.Input
{
    public enum InputEventType { Pressed, Released, Held }

    public interface IInputDevice
    {
        void Update(double elapsedMilliseconds, BitArray actionFlags);
    }
}
