// UiPlaygroundScreen.cs
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
///     Exercises every UI widget: text entries (OS keyboard input), sliders, checkboxes,
///     variable selectors, buttons (incl. disabled), a tab view, and a draggable/scrollable
///     clipped window.
/// </summary>
public class UiPlaygroundScreen(AppHost appHost, GameServiceContainer services) : UiScreen(services)
{
    public static readonly string[] DifficultyOptions = ["Easy", "Normal", "Hard", "Nightmare"];

    public override void Initialize()
    {
        DemoTextures.EnsureUiElements(appHost, services.GetService<GraphicsDevice>());
        var font = appHost.ContentProvider.Get<SpriteFont>("default");
        var buttonTex = appHost.ContentProvider.Get<Texture2D>("ui_button");
        var arrowL = appHost.ContentProvider.Get<Texture2D>("ui_arrow_l");
        var arrowR = appHost.ContentProvider.Get<Texture2D>("ui_arrow_r");
        var toggleTrue = appHost.ContentProvider.Get<Texture2D>("ui_toggle_true");
        var toggleFalse = appHost.ContentProvider.Get<Texture2D>("ui_toggle_false");
        var arrowScale = DemoTextures.ScaleFor(arrowL, 36f);
        var toggleScale = DemoTextures.ScaleFor(toggleTrue, 30f);

        UiRoot.Color = new Color(18, 18, 26);

        UiRoot.Add(
            MainMenuScreen.MakeButton(
                appHost,
                font,
                buttonTex,
                "Back",
                () => appHost.ScreenManager.PopScreen()
            )
        );

        // ------- Left column: the widget rows -------
        var panel = new UiVBox
        {
            RelX = 0.04f,
            RelY = 0.05f,
            RelWidth = 0.52f,
            RelHeight = 0.9f,
            Gap = 0.06f,
        };
        UiRoot.Add(panel);

        panel.Add(
            new UiText(font, "UI Playground")
            {
                Align = Align.Center,
                Scale = 1.6f,
                Color = Color.White,
            }
        );

        var nameEntry = new UiTextEntry(font)
        {
            Placeholder = "Your name...",
            Color = Color.White,
            Padding = 8,
            Width = 300,
        };
        var passwordEntry = new UiTextEntry(font)
        {
            Placeholder = "Password...",
            IsPassword = true,
            Color = Color.White,
            Width = 300,
        };
        panel.Add(Row(font, "Name", nameEntry));
        panel.Add(Row(font, "Password", passwordEntry));

        panel.Add(
            new UiText(font, "Hello!")
            {
                Align = Align.Center,
                Color = Color.MonoGameOrange,
                TextProvider = () =>
                    string.IsNullOrEmpty(nameEntry.Text) ? "Hello!" : $"Hello, {nameEntry.Text}!",
            }
        );

        var volume = new UiSlider(interactive: true)
        {
            RelWidth = 0.4f,
            RelHeight = 1, // fill the row's height; ratio > 1 => horizontal
            Color = Color.MonoGameOrange,
            BgColor = new Color(10, 10, 10),
        };
        volume.Value = 0.5f;
        panel.Add(Row(font, "Volume", volume));

        var vertical = new UiSlider(interactive: true)
        {
            RelWidth = 0.02f,
            RelHeight = 1,
            Color = Color.CornflowerBlue,
            BgColor = new Color(10, 10, 10),
        };
        panel.Add(Row(font, "Vertical", vertical, rowHeight: 0.14f));

        var vsync = new UiCheckBox(toggleTrue, toggleFalse) { Scale = toggleScale };
        var showFps = new UiCheckBox(toggleTrue, toggleFalse) { Scale = toggleScale };
        showFps.State = true;
        var boxes = new UiHBox { Gap = 0.35f, RelHeight = 1 };
        boxes.Add(new UiText(font, "VSync") { Color = Color.White, Align = Align.CenterV });
        boxes.Add(vsync);
        boxes.Add(new UiText(font, "Show FPS") { Color = Color.White, Align = Align.CenterV });
        boxes.Add(showFps);
        panel.Add(Row(font, "Boxes", boxes));

        var difficulty = new UiVariableSelector<string>(arrowL, arrowR, font, DifficultyOptions)
        {
            TextColor = Color.White,
            ButtonScale = arrowScale,
        };
        panel.Add(Row(font, "Difficulty", difficulty));

        var numbers = new UiVariableSelector<int>(arrowL, arrowR, font, [1, 2, 3, 4, 5])
        {
            TextColor = Color.White,
            ButtonScale = arrowScale,
        };
        panel.Add(Row(font, "Number", numbers));

        var buttons = new UiHBox { Gap = 0.25f, RelHeight = 1 };
        buttons.Add(
            MainMenuScreen.MakeButton(
                appHost,
                font,
                buttonTex,
                "Click me",
                () => DemoAudio.PlayUi(appHost, "confirm")
            )
        );
        var disabled = MainMenuScreen.MakeButton(appHost, font, buttonTex, "Disabled", () => { });
        disabled.Disable = true;
        buttons.Add(disabled);
        panel.Add(Row(font, "Buttons", buttons));

        // ------- Right column: tab view -------
        var tabs = new UiTabView<UiButton.Sprite>(new Color(30, 30, 38))
        {
            RelX = 0.58f,
            RelY = 0.05f,
            RelWidth = 0.38f,
            RelHeight = 0.55f,
            ActiveTabColor = Color.MonoGameOrange,
        };
        var tabA = new UiContainer { Color = new Color(38, 38, 50) };
        var tabB = new UiContainer { Color = new Color(38, 38, 50) };
        // A scrollable composition: the tab frame itself is a UiScrollArea.
        var tabC = new UiScrollArea { Color = new Color(38, 38, 50) };
        tabs.Add(MainMenuScreen.MakeButton(appHost, font, buttonTex, "Entries", () => { }), tabA);
        tabs.Add(MainMenuScreen.MakeButton(appHost, font, buttonTex, "Buttons", () => { }), tabB);
        tabs.Add(MainMenuScreen.MakeButton(appHost, font, buttonTex, "Scroll", () => { }), tabC);
        tabs.Initialize(TabAxis.X, 0.16f);
        UiRoot.Add(tabs);

        // Tab A: a live mirror of the name field plus a selector.
        var tabARow1 = new UiVBox
        {
            Gap = 0.2f,
            RelWidth = 1,
            RelHeight = 1,
            Color = Color.Transparent,
        };
        tabARow1.Add(
            new UiText(font, "Name is")
            {
                Align = Align.Center,
                Color = Color.White,
                TextProvider = () => string.IsNullOrEmpty(nameEntry.Text) ? "..." : nameEntry.Text,
            }
        );
        tabARow1.Add(
            new UiVariableSelector<string>(arrowL, arrowR, font, DifficultyOptions)
            {
                TextColor = Color.White,
                ButtonScale = arrowScale,
                Align = Align.Center,
                RelWidth = 0.6f,
                RelHeight = 0.3f,
            }
        );
        tabA.Add(tabARow1);

        // Tab B: nested buttons.
        var tabBRow = new UiVBox
        {
            Gap = 0.15f,
            RelWidth = 1,
            RelHeight = 1,
            Color = Color.Transparent,
        };
        tabBRow.Add(
            MainMenuScreen.MakeButton(
                appHost,
                font,
                buttonTex,
                "Confirm",
                () => DemoAudio.PlayUi(appHost, "confirm")
            )
        );
        tabBRow.Add(MainMenuScreen.MakeButton(appHost, font, buttonTex, "Reset", () => { }));
        tabB.Add(tabBRow);

        // Tab C: a settings-style tab with wheel-scrollable rows in the clipped frame.
        var tabCRows = new UiVBox
        {
            Gap = 0.06f,
            RelWidth = 1,
            HSpace = 10,
            VSpace = 4,
            Height = 24 * 40,
        };
        for (var i = 1; i <= 24; i++)
            tabCRows.Add(
                new UiText(font, $"Scrollable tab row {i:00}") { Color = Color.White, RelWidth = 1 }
            );
        tabC.Add(tabCRows);

        // ------- Draggable, scrollable window demonstrating edge-accurate clipping -------
        var window = new UiWindow(font, "Scroll & Clip")
        {
            X = 760,
            Y = 450,
            Width = 420,
            Height = 240,
        };
        var rows = new UiVBox
        {
            Gap = 0.06f,
            RelWidth = 1,
            HSpace = 10,
            VSpace = 4,
            Height = 24 * 40,
        };
        for (var i = 1; i <= 24; i++)
            rows.Add(
                new UiText(font, $"Scrollable row {i:00}") { Color = Color.White, RelWidth = 1 }
            );
        window.AddContent(rows);
        UiRoot.Add(window);
    }

    /// <summary>Row with a west-aligned label on the left and a control filling the right side.</summary>
    private static UiHBox Row(
        SpriteFont font,
        string label,
        UiElement control,
        float rowHeight = 0.08f
    )
    {
        var row = new UiHBox { Gap = 0.05f, RelHeight = rowHeight };
        row.Add(
            new UiText(font, label)
            {
                Color = Color.White,
                Align = Align.W,
                RelWidth = 0.15f,
                RelHeight = 1,
            }
        );
        row.Add(control);
        return row;
    }
}
