// SettingsScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App;
using MonoKit.App.Display;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.App.Ui;

namespace MonoKit.Demo.Screens;

/// <summary>
///     Display and audio settings. Display changes apply live via
///     <see cref="DisplayController.ApplySettings" />; volumes are wired straight into the
///     <see cref="App.Content.Audio.AudioService" />.
/// </summary>
public class SettingsScreen(AppHost appHost, GameServiceContainer services) : UiScreen(services)
{
    private UiVariableSelector<WindowMode> _mode = null!;
    private UiVariableSelector<int> _msaa = null!;
    private UiVariableSelector<int> _rate = null!;
    private UiCheckBox _vsync = null!;
    private UiSlider _musicVolume = null!;
    private UiSlider _sfxVolume = null!;

    public override void Initialize()
    {
        DemoTextures.EnsureUiElements(appHost, services.GetService<GraphicsDevice>());
        var font = appHost.ContentProvider.Get<SpriteFont>("default");
        var buttonTex = appHost.ContentProvider.Get<Texture2D>("ui_button");
        var arrowL = appHost.ContentProvider.Get<Texture2D>("ui_arrow_l");
        var arrowR = appHost.ContentProvider.Get<Texture2D>("ui_arrow_r");
        var toggleTrue = appHost.ContentProvider.Get<Texture2D>("ui_toggle_true");
        var toggleFalse = appHost.ContentProvider.Get<Texture2D>("ui_toggle_false");
        var caps = appHost.DisplayController.Capabilities;
        var arrowScale = DemoTextures.ScaleFor(arrowL, 36f);
        var toggleScale = DemoTextures.ScaleFor(toggleTrue, 30f);

        UiRoot.Color = new Color(16, 16, 24);

        var panel = new UiVBox
        {
            RelX = 0.28f,
            RelY = 0.07f,
            RelWidth = 0.44f,
            RelHeight = 0.86f,
            Gap = 0.05f,
        };
        UiRoot.Add(panel);

        panel.Add(
            new UiText(font, "Settings")
            {
                Align = Align.Center,
                Scale = 1.6f,
                Color = Color.White,
            }
        );

        _mode = new UiVariableSelector<WindowMode>(
            arrowL,
            arrowR,
            font,
            Enum.GetValues<WindowMode>()
        )
        {
            TextColor = Color.White,
            ButtonScale = arrowScale,
        };
        _msaa = new UiVariableSelector<int>(arrowL, arrowR, font, caps.MultiSampleCounts)
        {
            TextColor = Color.White,
            ButtonScale = arrowScale,
        };
        _rate = new UiVariableSelector<int>(arrowL, arrowR, font, [0, 60, 120, 144, 240]) // 0 = uncapped
        {
            TextColor = Color.White,
            ButtonScale = arrowScale,
        };
        _vsync = new UiCheckBox(toggleTrue, toggleFalse)
        {
            Scale = toggleScale,
            State = true,
            Align = Align.CenterV,
        };
        var vsyncBox = new UiHBox();
        vsyncBox.Add(_vsync);

        panel.Add(Row(font, "Window mode", _mode));
        panel.Add(Row(font, "MSAA", _msaa));
        panel.Add(Row(font, "Frame rate", _rate));
        panel.Add(Row(font, "VSync", vsyncBox));

        _musicVolume = new UiSlider(interactive: true)
        {
            RelWidth = 0.35f,
            RelHeight = 1,
            Color = Color.MonoGameOrange,
        };
        _musicVolume.Value = appHost.AudioService.Music.Volume;
        panel.Add(Row(font, "Music", _musicVolume));

        _sfxVolume = new UiSlider(interactive: true)
        {
            RelWidth = 0.35f,
            RelHeight = 1,
            Color = Color.MonoGameOrange,
        };
        _sfxVolume.Value = 0.9f;
        panel.Add(Row(font, "Sound fx", _sfxVolume));

        var buttons = new UiHBox { Gap = 0.3f, RelHeight = 1 };
        buttons.Add(MainMenuScreen.MakeButton(appHost, font, buttonTex, "Apply", Apply));
        buttons.Add(
            MainMenuScreen.MakeButton(
                appHost,
                font,
                buttonTex,
                "Back",
                () => appHost.ScreenManager.PopScreen()
            )
        );
        panel.Add(Row(font, "", buttons, rowHeight: 0.12f));
    }

    public override void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        // Volumes apply live while sliding (0..1).
        appHost.AudioService.Music.Volume = _musicVolume.Value;
        DemoAudio.SetSfxVolume(appHost, _sfxVolume.Value);
        base.Update(elapsedMs, inputHandler, uiScale);
    }

    private void Apply() =>
        appHost.DisplayController.ApplySettings(
            new DisplaySettings(_mode.Value, _rate.Value, _vsync.State, _msaa.Value)
        );

    /// <summary>Row with a west-aligned label on the left and a control filling the right side.</summary>
    private static UiHBox Row(
        SpriteFont font,
        string label,
        UiElement control,
        float rowHeight = 0.09f
    )
    {
        var row = new UiHBox { Gap = 0.05f, RelHeight = rowHeight };
        if (!string.IsNullOrEmpty(label))
            row.Add(
                new UiText(font, label)
                {
                    Color = Color.White,
                    Align = Align.W,
                    RelWidth = 0.2f,
                }
            );
        row.Add(control);
        return row;
    }
}
