// SettingsManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Graphics;
using Newtonsoft.Json;
using System;

namespace GameEngine.Content
{
    [Serializable]
    public class SettingsManager
    {
        [JsonProperty]
        private SettingsState _settings = new(1, .9f, 1, 60, true, false, WindowMode.Windowed, .9f, 1);

        public SettingsState GetSettings() => _settings;

        public void SetSettings(SettingsState settings) => _settings = settings;
    }

    [Serializable]
    public readonly struct SettingsState(float masterVol, float musicVol, float sfxVol, int refRate, bool showFPS, bool vSync, WindowMode windowMode, float uiScale, float bloomEffect)
    {
        [JsonProperty] public readonly float MasterVolume = masterVol;
        [JsonProperty] public readonly float MusicVolume = musicVol;
        [JsonProperty] public readonly float SFXVolume = sfxVol;
        [JsonProperty] public readonly int RefreshRate = refRate;
        [JsonProperty] public readonly bool ShowFPS = showFPS;
        [JsonProperty] public readonly bool VSync = vSync;
        [JsonProperty] public readonly WindowMode WindowMode = windowMode;
        [JsonProperty] public readonly float UiScale = uiScale;
        [JsonProperty] public readonly float BloomIntensity = bloomEffect;
    }
}
