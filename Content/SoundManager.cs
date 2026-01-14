// SoundManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using MonoKit.Core.IO;

namespace MonoKit.Content;

public sealed class SoundManager(int maxInstances)
{
    private readonly int _maxInstances = maxInstances;
    private readonly Dictionary<string, SoundEffectInstance[]> _soundEffectInstances = new();
    private float _volume;

    public float Volume
    {
        get => _volume;
        set => _volume = float.Clamp(value, 0, 1);
    }

    public void LoadContent(ContentManager contentManager, string contentDirectory,
        ContentLoadingState contentLoadingState = null)
    {
        var fullContentDirectory = Path.Combine(contentManager.RootDirectory, contentDirectory);
        var files = FileUtils.GetAllFilesInDirectory(fullContentDirectory, SearchOption.AllDirectories);

        for (var i = 0; i < files.Length; i++)
        {
            var filePath = files[i];
            var directory = Path.GetDirectoryName(filePath);
            var soundId = Path.GetFileNameWithoutExtension(filePath);
            var relativePath = Path.GetRelativePath(contentManager.RootDirectory, directory);
            var contentPath = Path.Combine(relativePath, soundId);
            var sound = contentManager.Load<SoundEffect>(contentPath);
            var soundInstances = Enumerable.Range(0, _maxInstances)
                .Select(_ => sound.CreateInstance())
                .ToArray();
            _soundEffectInstances[soundId] = soundInstances;
            contentLoadingState?.AddLoaded(filePath);
        }
    }

    public void PlaySound(string soundId, float volume = 1f, float pan = 0f, bool loop = false, bool interpup = true)
    {
        if (!_soundEffectInstances.TryGetValue(soundId, out var soundInstances))
            return;

        volume *= _volume;
        pan = float.Clamp(pan, -1, 1);
        var stoppedSoundInstances = soundInstances.Where(s => s.State == SoundState.Stopped).ToArray();

        var instance = soundInstances.First();
        if (stoppedSoundInstances.Any() && !interpup)
            instance = stoppedSoundInstances.First();
        else
            instance.Stop();

        instance.Volume = volume;
        instance.Pan = pan;
        instance.IsLooped = loop;
        instance.Play();
    }

    private void IterateThroughInstances(Action<SoundEffectInstance> action)
    {
        foreach (var instances in _soundEffectInstances.Values)
        foreach (var instance in instances)
            action.Invoke(instance);
    }

    public void Pause()
    {
        IterateThroughInstances(instance =>
        {
            if (instance.State == SoundState.Playing)
                instance.Pause();
        });
    }

    public void Resume()
    {
        IterateThroughInstances(instance =>
        {
            if (instance.State == SoundState.Paused)
                instance.Resume();
        });
    }

    public void Stop()
    {
        IterateThroughInstances(instance =>
        {
            instance.IsLooped = false;
            instance.Stop();
        });
    }
}