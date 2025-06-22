// SettingsManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Engine.Graphics;
using Newtonsoft.Json;
using System;

namespace Engine.Runtime
{
    [Serializable]
    public class SettingsManager
    {
        [JsonProperty]
        private SettingsState _settings = new(1, .9f, 1, 60, true, 4, false, WindowMode.Borderless, .9f, 1);

        public SettingsState GetSettings() => _settings;

        public void SetSettings(SettingsState settings) => _settings = settings;
    }

    [Serializable]
    public readonly struct SettingsState(float masterVol, float musicVol, float sfxVol, int refRate, bool showFPS, int msaa, bool vSync, WindowMode windowMode, float uiScale, float particleMult)
    {
        [JsonProperty] public readonly float MasterVolume = masterVol;
        [JsonProperty] public readonly float MusicVolume = musicVol;
        [JsonProperty] public readonly float SFXVolume = sfxVol;
        [JsonProperty] public readonly int RefreshRate = refRate;
        [JsonProperty] public readonly bool ShowFPS = showFPS;
        [JsonProperty] public readonly int MSAA = msaa;
        [JsonProperty] public readonly bool VSync = vSync;
        [JsonProperty] public readonly WindowMode WindowMode = windowMode;
        [JsonProperty] public readonly float UiScale = uiScale;
        [JsonProperty] public readonly float ParticlesMultiplier = particleMult;
    }
}
