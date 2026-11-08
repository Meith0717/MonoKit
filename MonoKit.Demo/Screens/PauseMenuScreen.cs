// PauseMenuScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App;
using MonoKit.App.Screens;
using MonoKit.App.Ui;

namespace MonoKit.Demo.Screens;

/// <summary>
///     Pause overlay: pushed on top of the gameplay screen. The game keeps drawing below
///     (drawBelow) but stops updating (updateBelow), demonstrating the screen-stack semantics.
/// </summary>
public class PauseMenuScreen(AppHost appHost, GameServiceContainer services)
    : UiScreen(services, updateBelow: false, drawBelow: true)
{
    public override void Initialize()
    {
        var font = appHost.ContentProvider.Get<SpriteFont>("default");
        var buttonTex = appHost.ContentProvider.Get<Texture2D>("ui_button");

        // Dim the frozen game behind the overlay.
        UiRoot.Color = new Color(10, 10, 18, 140);

        var menu = new UiVBox
        {
            RelX = 0.35f,
            RelY = 0.3f,
            RelWidth = 0.3f,
            RelHeight = 0.4f,
            Gap = 0.12f,
        };
        UiRoot.Add(menu);

        menu.Add(
            new UiText(font, "Paused")
            {
                Align = Align.Center,
                Scale = 1.5f,
                Color = Color.White,
            }
        );
        menu.Add(
            MainMenuScreen.MakeButton(appHost, font, buttonTex, "Resume", () => appHost.ScreenManager.PopScreen())
        );
        menu.Add(
            MainMenuScreen.MakeButton(
                appHost,
                font,
                buttonTex,
                "Settings",
                () => appHost.ScreenManager.AddScreen(new SettingsScreen(appHost, services))
            )
        );
        menu.Add(
            MainMenuScreen.MakeButton(appHost, font, buttonTex, "Quit to menu", QuitToMenu)
        );
    }

    private void QuitToMenu()
    {
        // Pop the pause overlay and the gameplay screen; lands back on the main menu.
        appHost.ScreenManager.PopScreen();
        appHost.ScreenManager.PopScreen();
    }
}
