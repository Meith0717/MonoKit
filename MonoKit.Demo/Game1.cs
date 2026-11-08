// Game1.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.App;
using MonoKit.App.Content;
using MonoKit.App.Core.IO;
using MonoKit.App.Display;
using MonoKit.App.Input;
using MonoKit.App.Input.Listeners;
using MonoKit.Demo.Screens;

namespace MonoKit.Demo;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private readonly AppHost _appHost;
    private SpriteBatch _spriteBatch = null!;

    public Game1()
    {
        var graphics = new GraphicsDeviceManager(this);
        _appHost = new AppHost(this, graphics)
        {
            PathService = new PathService("MonoKit.Demo", Environment.SpecialFolder.MyDocuments),
        };
        _appHost.DisplayController.ApplySettings(new DisplaySettings(WindowMode.Windowed, 60, vSync: true));

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _appHost.InputHandler.RegisterDevice(
            new MouseListener(new Dictionary<(MouseButton, InputEventType), byte>())
        );
        _appHost.InputHandler.RegisterDevice(
            new KeyboardListener(
                new Dictionary<(Keys, InputEventType), byte>
                {
                    { (Keys.W, InputEventType.Held), (byte)DemoAction.Up },
                    { (Keys.Up, InputEventType.Held), (byte)DemoAction.Up },
                    { (Keys.S, InputEventType.Held), (byte)DemoAction.Down },
                    { (Keys.Down, InputEventType.Held), (byte)DemoAction.Down },
                    { (Keys.A, InputEventType.Held), (byte)DemoAction.Left },
                    { (Keys.Left, InputEventType.Held), (byte)DemoAction.Left },
                    { (Keys.D, InputEventType.Held), (byte)DemoAction.Right },
                    { (Keys.Right, InputEventType.Held), (byte)DemoAction.Right },
                    { (Keys.Escape, InputEventType.Pressed), (byte)DemoAction.Back },
                }
            )
        );
    }

    protected override void Initialize()
    {
        // Boot content: needed before the first frame (loading screen included).
        ContentLoader
            .Create(Content)
            .RegisterContentDirectory<SpriteFont>(Path.Combine(ContentLayout.Boot, ContentLayout.Fonts))
            .RegisterContentDirectory<Texture2D>(Path.Combine(ContentLayout.Boot, ContentLayout.Textures))
            .LoadAll(_appHost.ContentProvider);

        base.Initialize();
        _appHost.Initialize(GraphicsDevice);

        // Everything else loads incrementally behind the loading screen.
        var loader = ContentLoader
            .Create(Content)
            .RegisterContentDirectory<Texture2D>(ContentLayout.Textures)
            .BeginIncremental(_appHost.ContentProvider);
        _appHost.ScreenManager.AddScreen(new LoadingScreen(_appHost, Services, loader));
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        _appHost.Update(gameTime, GraphicsDevice);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _appHost.Draw(gameTime, GraphicsDevice, _spriteBatch);
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        _appHost.Dispose();
        base.Dispose(disposing);
    }
}
