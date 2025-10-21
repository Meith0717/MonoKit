// inputHandler.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

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
        LeftClickHeld,
        RightClickHeld,
        MidClickHeld,
        LeftClick,
        RightClick,
        MidClick,
        MouseWheelForward,
        MouseWheelBackward,

        // DEBUG
        ReloadUi
    }
}
