// MonoKit - WindowMode.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace MonoKit.App.Display;

/// <summary>Window display mode, applied via <see cref="DisplayController.ApplyMode" />.</summary>
public enum WindowMode
{
    /// <summary>Exclusive fullscreen at the monitor's native resolution.</summary>
    FullScreen,

    /// <summary>Fullscreen window without hardware mode switch (no resolution change).</summary>
    Borderless,

    /// <summary>Resizable window restored to its previous size.</summary>
    Windowed,
}
