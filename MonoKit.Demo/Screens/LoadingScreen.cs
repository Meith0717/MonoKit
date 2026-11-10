// LoadingScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App;
using MonoKit.App.Content;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.App.Ui;

namespace MonoKit.Demo.Screens;

/// <summary>
///     Loads the non-boot content one file per frame behind a progress label and bar,
///     then opens the main menu.
/// </summary>
public class LoadingScreen(AppHost appHost, GameServiceContainer services, ContentLoader loader)
    : UiScreen(services)
{
    private UiText _progress = null!;
    private UiSlider _progressBar = null!;
    private bool _done;
    private readonly GameServiceContainer _services = services;

    public override void Initialize()
    {
        var font = appHost.ContentProvider.Get<SpriteFont>("default");
        var bg = appHost.ContentProvider.Get<Texture2D>("logo_background");

        UiRoot.AttachTexture(bg);
        UiRoot.FillScale = FillScale.Fit;

        _progressBar = new UiSlider(interactive: false)
        {
            Align = Align.S,
            VSpace = 50,
            RelWidth = 0.9f,
            RelHeight = 0.03f,
        };
        UiRoot.Add(_progressBar);

        _progress = new UiText(font, "Loading... 0%")
        {
            Align = Align.S,
            VSpace = 55,
            Scale = 1.5f,
            RelWidth = 1,
            RelHeight = 0.05f,
            Color = Color.LightGray,
        };
        UiRoot.Add(_progress);
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        base.Update(elapsedMilliseconds, inputHandler, uiScale);

        if (_done)
            return;

        var (_, percentage) = loader.LoadNext();
        _progressBar.Value = percentage;
        _progress.Text = $"Loading... {(int)(percentage * 100)}%";

        if (percentage < 1f)
            return;

        _done = true;
        loader.EndIncremental();
        appHost.ScreenManager.PopScreen();
        appHost.ScreenManager.AddScreen(new MainMenuScreen(appHost, _services));
    }
}
