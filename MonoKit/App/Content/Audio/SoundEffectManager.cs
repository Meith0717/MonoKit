// MonoKit - SoundEffectManager.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

namespace MonoKit.App.Content.Audio;

public sealed class SoundEffectManager(
    ContentProvider contentProvider,
    string category,
    int maxInstances = 10
)
{
    private readonly string _categoryPrefix = category.EndsWith("/") ? category : category + "/";
    private readonly Dictionary<string, SoundEffectInstance[]> _instances = new();
    private bool _isDisposed;
    private float _volume = 1f;

    public string Category => _categoryPrefix.TrimEnd('/');

    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0, 1);
    }

    /// <summary>Plays a sound, reusing stopped instances up to the limit.</summary>
    public void Play(
        string soundId,
        float volume = 1f,
        float pan = 0f,
        bool loop = false,
        bool interrupt = true
    )
    {
        if (_isDisposed)
            return;

        if (!_instances.TryGetValue(soundId, out var instances))
        {
            var fullId = _categoryPrefix + soundId;
            var soundEffect = contentProvider.Get<SoundEffect>(fullId);
            instances = Enumerable
                .Range(0, maxInstances)
                .Select(_ => soundEffect.CreateInstance())
                .ToArray();
            _instances[soundId] = instances;
        }

        volume *= _volume;
        pan = Math.Clamp(pan, -1, 1);

        var stoppedInstances = instances.Where(s => s.State == SoundState.Stopped).ToArray();

        var instance = instances[0];
        if (stoppedInstances.Length != 0 && !interrupt)
            instance = stoppedInstances[0];
        else
            instance.Stop();

        instance.Volume = volume;
        instance.Pan = pan;
        instance.IsLooped = loop;
        instance.Play();
    }

    /// <summary>Creates the sound instances up front for one sound id.</summary>
    public void Preload(string soundId)
    {
        if (_instances.ContainsKey(soundId))
            return;
        var fullId = _categoryPrefix + soundId;
        var soundEffect = contentProvider.Get<SoundEffect>(fullId);
        var instances = Enumerable
            .Range(0, maxInstances)
            .Select(_ => soundEffect.CreateInstance())
            .ToArray();
        _instances[soundId] = instances;
    }

    /// <summary>Preloads every sound effect in this manager's category.</summary>
    public void PreloadAll()
    {
        foreach (var (_, id, _) in contentProvider.Container<SoundEffect>())
        {
            if (!id.StartsWith(_categoryPrefix))
                continue;

            var soundId = id.Substring(_categoryPrefix.Length);
            Preload(soundId);
        }
    }

    /// <summary>Pauses all currently playing sound instances.</summary>
    public void Pause()
    {
        foreach (var instances in _instances.Values)
        foreach (var instance in instances)
            if (instance.State == SoundState.Playing)
                instance.Pause();
    }

    /// <summary>Resumes all paused sound instances.</summary>
    public void Resume()
    {
        foreach (var instances in _instances.Values)
        foreach (var instance in instances)
            if (instance.State == SoundState.Paused)
                instance.Resume();
    }

    /// <summary>Stops every sound instance of this category.</summary>
    public void Stop()
    {
        foreach (var instances in _instances.Values)
        foreach (var instance in instances)
        {
            instance.IsLooped = false;
            instance.Stop();
        }
    }

    /// <summary>Discards all cached sound instances.</summary>
    public void Clear()
    {
        _instances.Clear();
    }

    /// <summary>Clears instances and marks the manager as disposed.</summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        Clear();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
