// MonoKit - AudioService.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;

namespace MonoKit.App.Content.Audio;

public class AudioService : IDisposable
{
    private readonly Dictionary<string, SoundEffectManager> _sfxManagers = new();
    private ContentProvider _contentProvider;
    private bool _isDisposed;
    private MusicManager _musicManager;

    public MusicManager Music
    {
        get
        {
            _musicManager ??= new MusicManager(_contentProvider);
            return _musicManager;
        }
    }

    /// <summary>Stops playback and disposes all owned audio managers.</summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;
        StopAll();
        foreach (var manager in _sfxManagers.Values)
            manager.Dispose();
        _sfxManagers.Clear();
        _musicManager = null;
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>Binds the service to a content provider; call once at startup.</summary>
    public void Initialize(ContentProvider contentProvider)
    {
        if (_contentProvider != null)
            throw new InvalidOperationException("Audio service has already been initialized");
        _contentProvider = contentProvider;
    }

    /// <summary>Returns the manager for a category, creating it on first use.</summary>
    public SoundEffectManager GetOrCreateSfxManager(string category, int maxInstances = 10)
    {
        if (_sfxManagers.TryGetValue(category, out var manager))
            return manager;

        manager = new SoundEffectManager(_contentProvider, category, maxInstances);
        _sfxManagers[category] = manager;
        return manager;
    }

    /// <summary>Pauses playback in every sound effect manager.</summary>
    public void PauseAll()
    {
        foreach (var manager in _sfxManagers.Values)
            manager.Pause();
    }

    /// <summary>Resumes playback in every sound effect manager.</summary>
    public void ResumeAll()
    {
        foreach (var manager in _sfxManagers.Values)
            manager.Resume();
    }

    /// <summary>Stops playback in every sound effect manager.</summary>
    public void StopAll()
    {
        foreach (var manager in _sfxManagers.Values)
            manager.Stop();
    }

    /// <summary>Applies the same volume to every sound effect manager.</summary>
    public void SetAllVolume(float volume)
    {
        foreach (var manager in _sfxManagers.Values)
            manager.Volume = volume;
    }
}
