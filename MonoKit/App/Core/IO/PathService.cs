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

    public PathService RegisterPath(byte id, string relativePath)
    {
        var fullPath = Path.Combine(RootPath, relativePath);
        FileUtils.CreateDirectory(fullPath);
        _paths[id] = fullPath;
        return this;
    }

    public string GetPath(byte id)
    {
        return _paths.GetValueOrDefault(id);
    }

    public string GetFilePath(byte id, string fileName)
    {
        return !_paths.TryGetValue(id, out var path)
            ? throw new InvalidOperationException($"No path registered with id '{id}'.")
            : Path.Combine(path, fileName);
    }
}
