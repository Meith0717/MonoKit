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

        UiRoot.Add(
            new UiText(font, "MonoKit Demo")
            {
                Align = Align.Center,
                RelY = 0.1f,
                Scale = 2,
                Color = Color.White,
            }
        );

        var menu = new UiVBox
        {
            RelX = 0.35f,
            RelY = 0.35f,
            RelWidth = 0.3f,
            RelHeight = 0.42f,
            Gap = 0.1f,
        };
        UiRoot.Add(menu);

        menu.Add(MakeButton(appHost, font, buttonTex, "Play", OpenGameplay, targetWidth: 280f));
        menu.Add(
            MakeButton(appHost, font, buttonTex, "UI Playground", OpenPlayground, targetWidth: 280f)
        );
        menu.Add(
            MakeButton(
                appHost,
                font,
                buttonTex,
                "Settings",
                () => appHost.ScreenManager.AddScreen(new SettingsScreen(appHost, services)),
                targetWidth: 280f
            )
        );
        menu.Add(
            MakeButton(appHost, font, buttonTex, "Quit", () => appHost.ScreenManager.Exit(), targetWidth: 280f)
        );

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

    /// <summary>
    ///     Sprite button with a centered label, click/hover sounds, and art normalized to a
    ///     target pixel width (independent of the source texture's resolution).
    /// </summary>
    internal static UiButton.Sprite MakeButton(
        AppHost appHost,
        SpriteFont font,
        Texture2D texture,
        string label,
        Action onClick,
        float targetWidth = 300f
    ) =>
        new(texture)
        {
            Scale = DemoTextures.ScaleFor(texture, targetWidth),
            UiText = new UiText(font, label) { Align = Align.Center, Color = Color.Black },
            OnClickAction = () =>
            {
                DemoAudio.PlayUi(appHost, "click");
                onClick();
            },
            OnHoverAction = () => DemoAudio.PlayUi(appHost, "hover"),
        };
}
