// InputState.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace GameEngine.Input
{
    public enum ActionType : byte
    {
        ESC,
        Accelerate,
        Boost,
        Break,
        SwitchWeapon,
        OpenCargo,
        ObjectInteraction,
        OpenMap,
        Space,
        ScrollUp,
        ScrollDown,

        // Mouse
        LeftReleased,
        RightReleased,
        MidReleased,
        LeftClickHold,
        RightClickHold,
        MidClickHold,
        LeftWasClicked,
        RightWasClicked,
        MidWasClicked,
        MouseWheelForward,
        MouseWheelBackward,

        // DEBUG
        ReloadUi
    }

    public enum KeyEventType
    {
        OnButtonDown,
        OnButtonPressed
    }

    public class InputState()
    {
        public Vector2 MousePosition { get; private set; }
        private HashSet<ActionType> _actions = new();

        public void UpdateData(List<ActionType> actionTypes, Vector2 mousePosition)
        {
            _actions.Clear();
            for (int i = 0; i < actionTypes.Count; i++)
                _actions.Add(actionTypes[i]);
            MousePosition = mousePosition;
        }

        public bool HasAction(ActionType action) => _actions.Remove(action);

        public void DoAction(ActionType action, Action function)
        {
            if (function is null) return;
            if (HasAction(action)) function();
        }
    }
}
