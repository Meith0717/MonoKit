// MonoKit - AudioService.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;

namespace MonoKit.App.Content.Audio;

public class AudioService(ContentProvider contentProvider) : IDisposable
{
    private readonly Dictionary<string, SoundEffectManager> _sfxManagers = new();
    private bool _isDisposed;
    private MusicManager _musicManager;

    public MusicManager Music
    {
        get
        {
            _musicManager ??= new MusicManager(contentProvider);
            return _musicManager;
        }
    }

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

    public SoundEffectManager GetOrCreateSfxManager(string category, int maxInstances = 10)
    {
        if (_sfxManagers.TryGetValue(category, out var manager))
            return manager;

        manager = new SoundEffectManager(contentProvider, category, maxInstances);
        _sfxManagers[category] = manager;
        return manager;
    }

    public void PauseAll()
    {
        foreach (var manager in _sfxManagers.Values)
            manager.Pause();
    }

    public void ResumeAll()
    {
        foreach (var manager in _sfxManagers.Values)
            manager.Resume();
    }

    public void StopAll()
    {
        foreach (var manager in _sfxManagers.Values)
            manager.Stop();
    }

    public void SetAllVolume(float volume)
    {
        foreach (var manager in _sfxManagers.Values)
            manager.Volume = volume;
    }
}
