// MainMenuScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App;
using MonoKit.App.Screens;
using MonoKit.App.Ui;

namespace MonoKit.Demo.Screens;

/// <summary>Main menu: logo, buttons to all demo screens, menu music, FPS corner overlay.</summary>
public class MainMenuScreen(AppHost appHost, GameServiceContainer services) : UiScreen(services)
{
    public override void Initialize()
    {
        var font = appHost.ContentProvider.Get<SpriteFont>("default");
        var bg = appHost.ContentProvider.Get<Texture2D>("space_bg");
        var buttonTex = appHost.ContentProvider.Get<Texture2D>("ui_button");

        UiRoot.AttachTexture(bg);
        UiRoot.FillScale = FillScale.Fit;

        UiRoot.Add(
            new UiText(font, "MonoKit Demo")
            {
                Align = Align.NW,
                HSpace = 50,
                VSpace = 50,
                Scale = 2,
                Color = Color.White,
            }
        );

        var menu = new UiVBox
        {
            Align = Align.W,
            HSpace = 50,
            RelWidth = 0.3f,
            RelHeight = 0.45f,
            Gap = 0.15f,
        };
        UiRoot.Add(menu);

        menu.Add(MakeButton(appHost, font, buttonTex, "Play", OpenGameplay));
        menu.Add(MakeButton(appHost, font, buttonTex, "UI Playground", OpenPlayground));
        menu.Add(
            MakeButton(
                appHost,
                font,
                buttonTex,
                "Settings",
                () => appHost.ScreenManager.AddScreen(new SettingsScreen(appHost, services))
            )
        );
        menu.Add(MakeButton(appHost, font, buttonTex, "Quit", () => appHost.ScreenManager.Exit()));

        UiRoot.Add(
            new UiText(font)
            {
                TextProvider = () => $"{appHost.FrameCounter.CurrentFramesPerSecond:F0} fps",
                Align = Align.NW,
                Color = Color.LightGray,
            }
        );

        DemoAudio.PlaySong(appHost, "Sci-Fi 1");
    }

    private void OpenGameplay()
    {
        // The gameplay screen has no music of its own yet; stop the menu track when entering.
        appHost.AudioService.Music.Stop();
        appHost.ScreenManager.AddScreen(new GameplayScreen(appHost, services));
    }

    private void OpenPlayground()
    {
        appHost.ScreenManager.AddScreen(new UiPlaygroundScreen(appHost, services));
    }

    /// <summary>Sprite button with a centered label and click/hover sounds.</summary>
    internal static UiButton.Sprite MakeButton(
        AppHost appHost,
        SpriteFont font,
        Texture2D texture,
        string label,
        Action onClick
    ) =>
        new(texture)
        {
            UiText = new UiText(font, label) { Align = Align.Center, Color = Color.Black },
            OnClickAction = () =>
            {
                DemoAudio.PlayUi(appHost, "click");
                onClick();
            },
            OnHoverAction = () => DemoAudio.PlayUi(appHost, "hover"),
        };
}
