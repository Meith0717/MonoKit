// DemoAudio.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using MonoKit.App;
using MonoKit.App.Content;

namespace MonoKit.Demo;

/// <summary>
///     Sound wiring for the demo: plays UI sound effects and music when the content is present.
///     Content that has not been provided yet is skipped gracefully (GetId instead of Get,
///     which would show a "missing content" dialog).
/// </summary>
public static class DemoAudio
{
    public static void PlayUi(AppHost appHost, string soundId)
    {
        if (appHost.ContentProvider.Container<SoundEffect>().GetId("ui/" + soundId) < 0)
            return;
        appHost.AudioService.GetOrCreateSfxManager("ui").Preload(soundId);
        appHost.AudioService.GetOrCreateSfxManager("ui").Play(soundId);
    }

    public static void PlaySong(AppHost appHost, string songId)
    {
        if (appHost.ContentProvider.Container<Song>().GetId(songId) < 0)
            return;
        appHost.AudioService.Music.Play(songId);
    }

    public static void SetSfxVolume(AppHost appHost, float volume)
    {
        appHost.AudioService.GetOrCreateSfxManager("ui").Volume = volume;
    }
}
