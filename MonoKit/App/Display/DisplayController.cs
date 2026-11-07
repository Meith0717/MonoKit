// DisplayController.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Display;

/// <summary>
/// Manages the game window: display mode, backbuffer resolution, refresh rate/vsync,
/// and the scale from a virtual resolution to the current viewport.
/// </summary>
public class DisplayController : IDisposable
{
    private readonly Game _game;
    private readonly GraphicsDeviceManager _graphicsDeviceManager;
    private readonly GameWindow _window;
    private readonly float _virtualWidth;
    private readonly float _virtualHeight;
    private int _oldHeight;
    private int _oldWidth;
    private bool _resolutionWasResized;

    /// <param name="virtualWidth">Virtual resolution width used for <see cref="ViewportScale"/>.</param>
    /// <param name="virtualHeight">Virtual resolution height used for <see cref="ViewportScale"/>.</param>
    public DisplayController(
        Game game,
        GraphicsDeviceManager graphicsManager,
        float virtualWidth = 1920,
        float virtualHeight = 1080
    )
    {
        _game = game;
        _graphicsDeviceManager = graphicsManager;
        _graphicsDeviceManager.ApplyChanges();
        _window = game.Window;
        _virtualWidth = virtualWidth;
        _virtualHeight = virtualHeight;

        _window.ClientSizeChanged += OnClientSizeChanged;

        _oldWidth = _window.ClientBounds.Width;
        _oldHeight = _window.ClientBounds.Height;
        UpdateViewportScale();
    }

    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        _oldWidth = _window.ClientBounds.Width;
        _oldHeight = _window.ClientBounds.Height;
        UpdateViewportScale();
        _resolutionWasResized = true;
    }

    private void UpdateViewportScale()
    {
        var viewport = _graphicsDeviceManager.GraphicsDevice.Viewport.Bounds.Size;
        ViewportScale = float.Min(viewport.X / _virtualWidth, viewport.Y / _virtualHeight);
    }

    /// <summary>Scale factor from the virtual resolution to the current viewport.</summary>
    public float ViewportScale { get; private set; }

    /// <summary>True once after the window was resized; reading the property resets the flag.</summary>
    public bool ResolutionWasResized
    {
        get
        {
            var resized = _resolutionWasResized;
            _resolutionWasResized = false;
            return resized;
        }
    }

    private static Point MonitorSize =>
        new(
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
        );

    /// <summary>Sets the target refresh rate in Hz; when <paramref name="vSync"/> is on, the rate is ignored.</summary>
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

    /// <summary>Applies a full display configuration: refresh rate/vsync, then window mode.</summary>
    public void ApplySettings(DisplaySettings settings)
    {
        ApplyRefreshRate(settings.RefreshRate, settings.VSync);
        ApplyMode(settings.Mode);
    }

    /// <summary>Switches between windowed, borderless and fullscreen.</summary>
    public void ApplyMode(WindowMode mode)
    {
        switch (mode)
        {
            case WindowMode.Windowed:
                DoWindowMode();
                break;
            case WindowMode.FullScreen:
                DoFullScreen();
                break;
            case WindowMode.Borderless:
                DoBorderlessWindowed();
                break;
        }

        _graphicsDeviceManager.ApplyChanges();
        _resolutionWasResized = true;
    }

    private void DoFullScreen()
    {
        _graphicsDeviceManager.PreferredBackBufferHeight = MonitorSize.Y;
        _graphicsDeviceManager.PreferredBackBufferWidth = MonitorSize.X;

        _graphicsDeviceManager.HardwareModeSwitch = true;
        _graphicsDeviceManager.IsFullScreen = true;
    }

    private void DoBorderlessWindowed()
    {
        _graphicsDeviceManager.PreferredBackBufferHeight = MonitorSize.Y;
        _graphicsDeviceManager.PreferredBackBufferWidth = MonitorSize.X;

        _graphicsDeviceManager.HardwareModeSwitch = false;
        _graphicsDeviceManager.IsFullScreen = true;
    }

    private void DoWindowMode()
    {
        _graphicsDeviceManager.PreferredBackBufferHeight = _oldHeight;
        _graphicsDeviceManager.PreferredBackBufferWidth = _oldWidth;

        _graphicsDeviceManager.IsFullScreen = false;
        _window.AllowUserResizing = true;
    }

    /// <summary>Unsubscribes from the window's size-changed event.</summary>
    public void Dispose()
    {
        _window.ClientSizeChanged -= OnClientSizeChanged;
    }
}
