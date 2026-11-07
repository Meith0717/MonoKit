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

public class AppHost(Game game, GraphicsDeviceManager graphicsDeviceManager) : IDisposable
{
    public readonly ContentProvider ContentProvider = new();
    public readonly AudioService AudioService = new();
    public readonly InputHandler InputHandler = new();
    public readonly ScreenManager ScreenManager = new(game);
    public readonly DisplayController DisplayController = new(game, graphicsDeviceManager);

    public PathService PathService { get; init; }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        AudioService.Initialize(ContentProvider);

        var services = game.Services;
        services.AddService(PathService);
        services.AddService(ContentProvider);
        services.AddService(AudioService);
        services.AddService(InputHandler);
        services.AddService(DisplayController);
        services.AddService(ScreenManager);
    }

    public void Update(GameTime gameTime)
    {
        var elapsedMs = gameTime.ElapsedGameTime.TotalMilliseconds;
        InputHandler.Update(elapsedMs);
        ScreenManager.Update(elapsedMs, InputHandler, DisplayController.ViewportScale);
    }

    public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        ScreenManager.Draw(spriteBatch, graphicsDevice);
    }

    public void Dispose()
    {
        AudioService.Dispose();
    }
}
