// MonoKit - ContentLayout.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.IO;
using MonoKit.App.Core.IO;

// ReSharper disable MemberCanBePrivate.Global

namespace MonoKit.App.Content;

public static class ContentLayout
{
    public const string Textures = "textures";
    public const string Effects = "effects";
    public const string Fonts = "fonts";
    public const string Music = "music";
    public const string SoundEffects = "soundEffects";
    public const string Prefabs = "prefabs";
    public const string Boot = "boot";

    /// <summary>
    /// Walks up from the app base directory to find the project's Content folder
    /// (the one containing the .mgcb project file). Returns null when not found.
    /// </summary>
    public static string? FindProjectContentRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Content");
            if (Directory.Exists(candidate) && Directory.GetFiles(candidate, "*.mgcb").Length > 0)
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    public static void CreateContentLayout(string contentRoot)
    {
        // Runtime content
        FileUtils.CreateDirectory(Path.Combine(contentRoot, Textures));
        FileUtils.CreateDirectory(Path.Combine(contentRoot, Effects));
        FileUtils.CreateDirectory(Path.Combine(contentRoot, Fonts));
        FileUtils.CreateDirectory(Path.Combine(contentRoot, Music));
        FileUtils.CreateDirectory(Path.Combine(contentRoot, SoundEffects));
        FileUtils.CreateDirectory(Path.Combine(contentRoot, Prefabs));

        // Boot content
        FileUtils.CreateDirectory(Path.Combine(contentRoot, Boot, Textures));
        FileUtils.CreateDirectory(Path.Combine(contentRoot, Boot, Effects));
        FileUtils.CreateDirectory(Path.Combine(contentRoot, Boot, Fonts));
    }
}
