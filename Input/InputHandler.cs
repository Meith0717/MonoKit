// NewInputManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;

namespace GameEngine.Input
{
    public enum InputEventType { Pressed, Released, Held }

    public record InputEvent<TActionType>(TActionType Action, InputEventType Type);

    public interface IInputDevice<TActionType>
    {
        void Update(double elapsedMilliseconds, List<TActionType> actions);
    }

    public class InputHandler<TActionType>
    {
        private readonly List<IInputDevice<TActionType>> _devices = new();
        private readonly List<TActionType> _actions = new();

        public void RegisterDevice(IInputDevice<TActionType> device) => _devices.Add(device);

        public void Update(double elapsedMilliseconds)
        {
            _actions.Clear();
            foreach (var device in _devices)
                device.Update(elapsedMilliseconds, _actions);
        }
    }
}
