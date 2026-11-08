// AppHost - DisplayController.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Display;

/// <summary>
///     Owns the game window configuration: display mode, backbuffer resolution, refresh
///     rate/vsync, and the scale from a virtual resolution to the current backbuffer.
///     Safe to construct inside the Game constructor — no GraphicsDevice is touched until
///     one exists.
/// </summary>
public sealed class DisplayController : IDisposable
{
    private readonly Microsoft.Xna.Framework.Game _game;
    private readonly GraphicsDeviceManager _graphicsDeviceManager;
    private readonly GameWindow _window;
    private float _virtualWidth;
    private float _virtualHeight;

    // Last windowed size, restored when switching back from fullscreen/borderless.
    private int _restoreWidth;
    private int _restoreHeight;

    /// <summary>Initializes the controller and hooks the window's size-changed event.</summary>
    public DisplayController(
        Microsoft.Xna.Framework.Game game,
        GraphicsDeviceManager graphicsDeviceManager,
        float virtualWidth = 1920,
        float virtualHeight = 1080
    )
    {
        _game = game;
        _graphicsDeviceManager = graphicsDeviceManager;
        _window = game.Window;
        _virtualWidth = virtualWidth;
        _virtualHeight = virtualHeight;
        _restoreWidth = _window.ClientBounds.Width;
        _restoreHeight = _window.ClientBounds.Height;

        _window.AllowUserResizing = true;
        _window.ClientSizeChanged += OnClientSizeChanged;
    }

    /// <summary>Current window mode; updated by <see cref="ApplyMode" />.</summary>
    public WindowMode Mode { get; private set; } = WindowMode.Windowed;

    /// <summary>Device capabilities for settings menus: MSAA counts, profiles, fullscreen modes, adapters.</summary>
    public DisplayCapabilities Capabilities { get; } = QueryCapabilities();

    /// <summary>Highest MSAA sample count the current adapter supports for the backbuffer (0 = none).</summary>
    public int SupportedMultiSampleCount =>
        Capabilities.MultiSampleCounts.Length > 1 ? Capabilities.MultiSampleCounts[0] : 0;

    /// <summary>Queries adapter MSAA counts, profiles, fullscreen modes and monitor descriptions.</summary>
    private static DisplayCapabilities QueryCapabilities()
    {
        var adapter = GraphicsAdapter.DefaultAdapter;

        var sampleCounts = new List<int>();
        foreach (var count in new[] { 16, 8, 4, 2 })
            if (
                adapter.QueryRenderTargetFormat(
                    GraphicsProfile.HiDef,
                    SurfaceFormat.Color,
                    DepthFormat.Depth24,
                    count,
                    out _,
                    out _,
                    out var qualityLevels
                ) && qualityLevels > 0
            )
                sampleCounts.Add(count);
        sampleCounts.Add(0);

        var profiles = new List<GraphicsProfile>();
        if (adapter.IsProfileSupported(GraphicsProfile.HiDef))
            profiles.Add(GraphicsProfile.HiDef);
        if (adapter.IsProfileSupported(GraphicsProfile.Reach))
            profiles.Add(GraphicsProfile.Reach);

        var modes = new List<DisplayMode>();
        foreach (var mode in adapter.SupportedDisplayModes)
            modes.Add(mode);

        var adapters = new List<string>();
        foreach (var a in GraphicsAdapter.Adapters)
            adapters.Add(a.Description);

        return new DisplayCapabilities(
            sampleCounts.ToArray(),
            profiles.ToArray(),
            modes.ToArray(),
            adapters.ToArray()
        );
    }

    /// <summary>Scale factor from the virtual resolution to the current backbuffer. 1 until a device exists.</summary>
    public float ViewportScale { get; private set; } = 1f;

    /// <summary>Raised whenever <see cref="ViewportScale" /> actually changes (window resize, mode switch).</summary>
    public event Action<float> ResolutionChanged;

    /// <summary>Changes the virtual resolution and recomputes the scale.</summary>
    public void SetVirtualResolution(float width, float height)
    {
        _virtualWidth = width;
        _virtualHeight = height;
        UpdateViewportScale();
    }

    /// <summary>
    ///     Recomputes <see cref="ViewportScale" /> from the current backbuffer. Null-safe:
    ///     before the GraphicsDevice exists the scale stays unchanged. Raises
    ///     <see cref="ResolutionChanged" /> only on an actual change.
    /// </summary>
    public void UpdateViewportScale()
    {
        var device = _graphicsDeviceManager.GraphicsDevice;
        if (device == null)
            return;

        var bounds = device.Viewport.Bounds.Size;
        SetScale(float.Min(bounds.X / _virtualWidth, bounds.Y / _virtualHeight));
    }

    /// <summary>Updates the scale and raises ResolutionChanged when it changed.</summary>
    private void SetScale(float scale)
    {
        if (scale.Equals(ViewportScale))
            return;

        ViewportScale = scale;
        ResolutionChanged?.Invoke(scale);
    }

    /// <summary>Remembers windowed sizes on resize and refreshes the viewport scale.</summary>
    private void OnClientSizeChanged(object? sender, EventArgs e)
    {
        // Only remember windowed sizes; a fullscreen resize must not overwrite them.
        if (!_graphicsDeviceManager.IsFullScreen)
        {
            _restoreWidth = _window.ClientBounds.Width;
            _restoreHeight = _window.ClientBounds.Height;
        }

        UpdateViewportScale();
    }

    /// <summary>Applies a full display configuration: refresh rate/vsync, then window mode.</summary>
    public void ApplySettings(DisplaySettings settings)
    {
        ApplyRefreshRate(settings.RefreshRate, settings.VSync);
        ApplyMultiSample(Math.Min(settings.MultiSampleCount, SupportedMultiSampleCount));
        ApplyMode(settings.Mode);
    }

    /// <summary>
    /// Sets the target refresh rate in Hz; ignored when <paramref name="vSync" /> is on.
    /// A value &lt;= 0 with vsync off runs uncapped (fixed timestep off, no frame limit).
    /// </summary>
    public void ApplyRefreshRate(int value, bool vSync)
    {
        _graphicsDeviceManager.SynchronizeWithVerticalRetrace = vSync;
        _game.IsFixedTimeStep = !vSync && value > 0;

        if (!vSync && value > 0)
            _game.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / value);

        _graphicsDeviceManager.ApplyChanges();
    }

    /// <summary>Sets MSAA on the backbuffer via the presentation parameters; clamps to <see cref="SupportedMultiSampleCount" />.</summary>
    public void ApplyMultiSample(int sampleCount)
    {
        var device = _graphicsDeviceManager.GraphicsDevice;
        if (device == null)
            return;

        sampleCount = Math.Min(sampleCount, SupportedMultiSampleCount);
        if (device.PresentationParameters.MultiSampleCount == sampleCount)
            return;

        var pp = device.PresentationParameters;
        pp.MultiSampleCount = sampleCount;
        device.Reset(pp);
    }

    /// <summary>Switches between windowed, borderless and fullscreen.</summary>
    public void ApplyMode(WindowMode mode)
    {
        Mode = mode;

        switch (mode)
        {
            case WindowMode.Windowed:
                _graphicsDeviceManager.PreferredBackBufferWidth = _restoreWidth;
                _graphicsDeviceManager.PreferredBackBufferHeight = _restoreHeight;
                _graphicsDeviceManager.IsFullScreen = false;
                _window.AllowUserResizing = true;
                break;
            case WindowMode.FullScreen:
                _graphicsDeviceManager.PreferredBackBufferWidth = MonitorSize.X;
                _graphicsDeviceManager.PreferredBackBufferHeight = MonitorSize.Y;
                _graphicsDeviceManager.HardwareModeSwitch = true;
                _graphicsDeviceManager.IsFullScreen = true;
                break;
            case WindowMode.Borderless:
                _graphicsDeviceManager.PreferredBackBufferWidth = MonitorSize.X;
                _graphicsDeviceManager.PreferredBackBufferHeight = MonitorSize.Y;
                _graphicsDeviceManager.HardwareModeSwitch = false;
                _graphicsDeviceManager.IsFullScreen = true;
                break;
        }

        _graphicsDeviceManager.ApplyChanges();
        UpdateViewportScale();
    }

    /// <summary>Resolution of the default monitor.</summary>
    private static Point MonitorSize =>
        new(
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
        );

    /// <summary>Unsubscribes from the window's size-changed event.</summary>
    public void Dispose()
    {
        _window.ClientSizeChanged -= OnClientSizeChanged;
    }
}
