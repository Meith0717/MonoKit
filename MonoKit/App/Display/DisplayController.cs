// AppHost - DisplayController.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
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
    private readonly Game _game;
    private readonly GraphicsDeviceManager _graphicsDeviceManager;
    private readonly GameWindow _window;
    private float _virtualWidth;
    private float _virtualHeight;

    // Last windowed size, restored when switching back from fullscreen/borderless.
    private int _restoreWidth;
    private int _restoreHeight;

    public DisplayController(
        Game game,
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

    private void SetScale(float scale)
    {
        if (scale.Equals(ViewportScale))
            return;

        ViewportScale = scale;
        ResolutionChanged?.Invoke(scale);
    }

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
        ApplyMode(settings.Mode);
    }

    /// <summary>Sets the target refresh rate in Hz; when <paramref name="vSync" /> is on, the rate is ignored.</summary>
    public void ApplyRefreshRate(int value, bool vSync)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        _graphicsDeviceManager.SynchronizeWithVerticalRetrace = vSync;
        _game.IsFixedTimeStep = !vSync;

        if (!vSync)
            _game.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / value);

        _graphicsDeviceManager.ApplyChanges();
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
