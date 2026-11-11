// MonoKit - SettingsManager.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using MonoKit.App.Display;
using Newtonsoft.Json;

namespace MonoKit.App.Content;

[Serializable]
public class SettingsManager
{
    [JsonProperty]
    private SettingsState _settings = new(
        1,
        .9f,
        1,
        60,
        true,
        false,
        WindowMode.Windowed,
        .9f,
        1,
        .5f
    );

    /// <summary>Returns the currently stored settings state.</summary>
    public SettingsState GetSettings()
    {
        return _settings;
    }

    /// <summary>Replaces the current settings state.</summary>
    public void SetSettings(SettingsState settings)
    {
        _settings = settings;
    }
}

[Serializable]
public readonly struct SettingsState(
    float masterVol,
    float musicVol,
    float sfxVol,
    int refRate,
    bool showFPS,
    bool vSync,
    WindowMode windowMode,
    float uiScale,
    float bloomEffect,
    float bloomThreshold
)
{
    [JsonProperty]
    public readonly float MasterVolume = masterVol;

    [JsonProperty]
    public readonly float MusicVolume = musicVol;

    [JsonProperty]
    public readonly float SFXVolume = sfxVol;

    [JsonProperty]
    public readonly int RefreshRate = refRate;

    [JsonProperty]
    public readonly bool ShowFPS = showFPS;

    [JsonProperty]
    public readonly bool VSync = vSync;

    [JsonProperty]
    public readonly WindowMode WindowMode = windowMode;

    [JsonProperty]
    public readonly float UiScale = uiScale;

    [JsonProperty]
    public readonly float BloomIntensity = bloomEffect;

    [JsonProperty]
    public readonly float BloomThreshold = bloomThreshold;
}
