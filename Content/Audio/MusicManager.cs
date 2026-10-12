// MusicManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework.Media;

namespace MonoKit.Content.Audio;

public sealed class MusicManager(ContentProvider contentProvider)
{
    private Song _currentSong;
    private bool _isDisposed;

    public float Volume
    {
        get => MediaPlayer.Volume;
        set => MediaPlayer.Volume = Math.Clamp(value, 0, 1);
    }

    public void Play(string soundId, bool loop = true)
    {
        if (_isDisposed)
            return;

        var song = contentProvider.Get<Song>(soundId);

        if (song == _currentSong && MediaPlayer.State == MediaState.Playing)
            return;

        MediaPlayer.IsRepeating = loop;
        MediaPlayer.Play(song);
        _currentSong = song;
    }

    public void Stop()
    {
        if (MediaPlayer.State == MediaState.Stopped)
            return;

        MediaPlayer.Stop();
        _currentSong = null;
    }

    public void Pause()
    {
        if (MediaPlayer.State == MediaState.Playing)
            MediaPlayer.Pause();
    }

    public void Resume()
    {
        if (MediaPlayer.State == MediaState.Paused)
            MediaPlayer.Resume();
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        Stop();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
