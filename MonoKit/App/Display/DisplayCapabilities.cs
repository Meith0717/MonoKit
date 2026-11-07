// DisplayCapabilities.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Display;

/// <summary>
/// Device capabilities for building settings menus: supported MSAA sample counts
/// (descending, always ending in 0 = off), supported graphics profiles, available
/// fullscreen display modes, and the installed adapters (monitors).
/// </summary>
public readonly struct DisplayCapabilities(
    int[] multiSampleCounts,
    GraphicsProfile[] graphicsProfiles,
    DisplayMode[] fullscreenModes,
    string[] adapterDescriptions
)
{
    /// <summary>Supported MSAA sample counts, descending; the last entry is always 0 (off).</summary>
    public readonly int[] MultiSampleCounts = multiSampleCounts;

    /// <summary>Graphics profiles supported by the default adapter.</summary>
    public readonly GraphicsProfile[] GraphicsProfiles = graphicsProfiles;

    /// <summary>Fullscreen display modes of the default adapter (width/height/format).</summary>
    public readonly DisplayMode[] FullscreenModes = fullscreenModes;

    /// <summary>Descriptions of all installed adapters, for multi-monitor selection.</summary>
    public readonly string[] AdapterDescriptions = adapterDescriptions;
}
