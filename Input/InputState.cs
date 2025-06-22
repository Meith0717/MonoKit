// InputState.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Engine.Input
{
    public enum ActionType
    {
        None,
        ESC,
        CameraZoomIn,
        CameraZoomOut,
        Accelerate,
        Boost,
        Break,
        MoveCameraLeft,
        MoveCameraRight,
        MoveCameraUp,
        MoveCameraDown,
        Space,
        MoveButtonUp,
        MoveButtonDown,

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

    }

    public enum GamePadActionType
    {
        None,
        LeftThumbStickUp,
        LeftThumbStickDown,
        RightThumbStickUp,
        RightThumbStickDown,
    }

    public enum KeyEventType
    {
        OnButtonDown,
        OnButtonPressed
    }

    public struct ThumbSticksState
    {
        public Vector2 LeftThumbSticks = Vector2.Zero;
        public Vector2 RightThumbSticks = Vector2.Zero;
        public float LeftTrigger = new();
        public float RightTrigger = new();

        public ThumbSticksState() {; }
    }

    public struct InputState
    {
        public List<ActionType> Actions = new();
        public Vector2 MousePosition = Vector2.Zero;
        public string TypedString = "";

        public InputState(List<ActionType> actions, string typedString, Vector2 mousePosition)
        {
            Actions = actions;
            TypedString = typedString;
            MousePosition = mousePosition;
        }

        public readonly bool HasAction(ActionType action) => Actions.Remove(action);

        public readonly void DoAction(ActionType action, Action function)
        {
            if (function is null) return;
            if (HasAction(action)) function();
        }
    }
}
