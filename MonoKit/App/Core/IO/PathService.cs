// AppHost - PathService.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.IO;
using static System.Environment;

namespace MonoKit.App.Core.IO;

public class PathService
{
    private readonly Dictionary<byte, string> _paths = new();

    /// <summary>Builds and creates the root path from a folder or relative name.</summary>
    public PathService(string rootPath = null, SpecialFolder? baseFolder = null)
    {
        var folderName = rootPath ?? "AppHost";

        var basePath = baseFolder.HasValue
            ? Path.Combine(GetFolderPath(baseFolder.Value), folderName)
            : Path.GetFullPath(folderName);

        RootPath = basePath;
        FileUtils.CreateDirectory(RootPath);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public string RootPath { get; }

    /// <summary>Creates a subdirectory and registers it under an id.</summary>
    public PathService RegisterPath(byte id, string relativePath)
    {
        var fullPath = Path.Combine(RootPath, relativePath);
        FileUtils.CreateDirectory(fullPath);
        _paths[id] = fullPath;
        return this;
    }

    /// <summary>Returns the registered path for an id, or null.</summary>
    public string GetPath(byte id)
    {
        return _paths.GetValueOrDefault(id);
    }

    /// <summary>Combines a registered path with a file name.</summary>
    public string GetFilePath(byte id, string fileName)
    {
        return !_paths.TryGetValue(id, out var path)
            ? throw new InvalidOperationException($"No path registered with id '{id}'.")
            : Path.Combine(path, fileName);
    }
}
