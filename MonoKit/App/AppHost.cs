// AppHost - AppHost.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Content.Audio;
using MonoKit.App.Core.Diagnostics;
using MonoKit.App.Core.IO;
using MonoKit.App.Display;
using MonoKit.App.Input;
using MonoKit.App.Screens;

// ReSharper disable MemberCanBePrivate.Global

namespace MonoKit.App;

public class AppHost(Microsoft.Xna.Framework.Game game, GraphicsDeviceManager graphicsDeviceManager)
    : IDisposable
{
    private bool _wasInitialized;
    public readonly FrameCounter FrameCounter = new();
    public readonly ContentProvider ContentProvider = new();
    public readonly AudioService AudioService = new();
    public readonly InputHandler InputHandler = new();
    public readonly ScreenManager ScreenManager = new(game);
    public readonly DisplayController DisplayController = new(game, graphicsDeviceManager);

    public PathService PathService { get; init; }

    /// <summary>Creates and starts the debug console, reusing an existing instance.</summary>
    public DebugConsole EnableDebugConsole()
    {
        if (game.Services.GetService(typeof(DebugConsole)) is DebugConsole console)
            return console;

        var debugConsole = new DebugConsole();
        game.Services.AddService(debugConsole);
        debugConsole.Start();
        return debugConsole;
    }

    /// <summary>Registers all host services; runs only once.</summary>
    public void Initialize(GraphicsDevice graphicsDevice)
    {
        if (_wasInitialized)
            return;
        _wasInitialized = true;

        AudioService.Initialize(ContentProvider);

        var services = game.Services;
        services.AddService(graphicsDevice);
        services.AddService(ContentProvider);
        services.AddService(AudioService);
        services.AddService(InputHandler);
        services.AddService(DisplayController);
        services.AddService(ScreenManager);
        services.AddService(FrameCounter);

        // OS text input (keyboard layout + key repeat) for text entry widgets.
        InputHandler.EnableTextInput(game.Window);

        if (PathService != null)
            services.AddService(PathService);

        DisplayController.ResolutionChanged += scale =>
        {
            ScreenManager.OnResolutionChanged(scale, graphicsDevice);
        };
    }

    /// <summary>Advances input, viewport scale and screens each frame.</summary>
    public void Update(GameTime gameTime, GraphicsDevice graphicsDevice)
    {
        if (!_wasInitialized)
            throw new InvalidOperationException("AppHost has not been initialized.");

        var elapsedMs = gameTime.ElapsedGameTime.TotalMilliseconds;
        DisplayController.UpdateViewportScale();
        InputHandler.Update(elapsedMs);
        ScreenManager.Update(elapsedMs, InputHandler, DisplayController.ViewportScale);
    }

    /// <summary>Updates frame statistics and draws the active screens.</summary>
    public void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        if (!_wasInitialized)
            throw new InvalidOperationException("AppHost has not been initialized.");

        FrameCounter.Update(
            gameTime.ElapsedGameTime.TotalSeconds,
            gameTime.ElapsedGameTime.TotalMilliseconds
        );
        ScreenManager.Draw(spriteBatch, graphicsDevice);
    }

    /// <summary>Releases the debug console, audio service and display controller.</summary>
    public void Dispose()
    {
        if (game.Services.GetService<DebugConsole>() is DebugConsole console)
            console.Dispose();

        AudioService.Dispose();
        DisplayController.Dispose();
    }
}
