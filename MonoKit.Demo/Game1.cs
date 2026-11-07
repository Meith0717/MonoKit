// AppHost.Demo - Game1.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App;
using MonoKit.App.Content;
using MonoKit.App.Core.IO;

namespace MonoKit.Demo;

public class Game1 : Game
{
    private readonly AppHost _appHost;
    private SpriteBatch _spriteBatch;

    public Game1()
    {
        var graphics = new GraphicsDeviceManager(this);
        _appHost = new AppHost(this, graphics)
        {
            PathService = new PathService("MonoKit.Demo", Environment.SpecialFolder.MyDocuments),
        };
        _appHost.DisplayController.ApplyRefreshRate(30, false);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        ContentLoader
            .Create(Content)
            .RegisterContentDirectory<SpriteFont>("fonts")
            .LoadAll(_appHost.ContentProvider);

        base.Initialize();
        _appHost.Initialize(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        // TODO: use this.Content to load your game content here
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

        _spriteBatch.Begin();
        _spriteBatch.DrawString(
            _appHost.ContentProvider.Get<SpriteFont>("default"),
            $"{double.Round(_appHost.FrameCounter.CurrentFramesPerSecond, 2)} fps",
            Vector2.Zero,
            Color.White
        );
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        _appHost.Dispose();
        base.Dispose(disposing);
    }
}
