// AudioService.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;

namespace MonoKit.Content;

public sealed class AudioService
{
    // Lazy-loaded singleton instance
    private static readonly Lazy<AudioService> _instance = new(() => new AudioService());

    // Internal content providers
    private readonly SoundManager _musicsInternal;
    private readonly SoundManager _soundEffectsInternal;

    // Private constructor
    private AudioService()
    {
        _musicsInternal = new SoundManager(1);
        _soundEffectsInternal = new SoundManager(10);
    }

    private static AudioService Instance => _instance.Value;

    // Static proxy accessors
    public static SoundManager Musics => Instance._musicsInternal;
    public static SoundManager SFX => Instance._soundEffectsInternal;
}