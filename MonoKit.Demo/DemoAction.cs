// DemoAction.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace MonoKit.Demo;

/// <summary>Byte-backed action enum, cast to the action ID used in input bindings.</summary>
public enum DemoAction : byte
{
    Up,
    Down,
    Left,
    Right,
    Back,
    Shoot,
    Pause,
}
