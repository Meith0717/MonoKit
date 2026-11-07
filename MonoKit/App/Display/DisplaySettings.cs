// AppHost - DisplaySettings.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace MonoKit.App.Display;

/// <summary>
///     Display configuration applied to a <see cref="DisplayController" /> in one call
///     via <see cref="DisplayController.ApplySettings" />. The virtual resolution is set
///     separately via the constructor or <see cref="DisplayController.SetVirtualResolution" />.
/// </summary>
public readonly struct DisplaySettings(WindowMode mode, int refreshRate, bool vSync)
{
    /// <summary>Windowed, borderless or fullscreen.</summary>
    public readonly WindowMode Mode = mode;

    /// <summary>Target refresh rate in Hz; ignored when <see cref="VSync" /> is on.</summary>
    public readonly int RefreshRate = refreshRate;

    /// <summary>Synchronize with the vertical retrace.</summary>
    public readonly bool VSync = vSync;
}
