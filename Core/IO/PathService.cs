// PathService.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using static System.Environment;

namespace MonoKit.Core.IO;

public class PathService<TPathId>
    where TPathId : Enum
{
    private readonly Dictionary<TPathId, string> _paths = new();

    public PathService(string rootPath = null, SpecialFolder? baseFolder = null)
    {
        var folderName = rootPath ?? "MonoKit";

        var basePath = baseFolder.HasValue
            ? Path.Combine(GetFolderPath(baseFolder.Value), folderName)
            : Path.GetFullPath(folderName);

        RootPath = basePath;
        FileUtils.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public void RegisterPath(TPathId id, string relativePath)
    {
        var fullPath = Path.Combine(RootPath, relativePath);
        FileUtils.CreateDirectory(fullPath);
        _paths[id] = fullPath;
    }

    public string GetPath(TPathId id)
    {
        return _paths.TryGetValue(id, out var path) ? path : null;
    }

    public string GetFilePath(TPathId id, string fileName)
    {
        if (!_paths.TryGetValue(id, out var path))
            throw new InvalidOperationException($"No path registered with id '{id}'.");

        return Path.Combine(path, fileName);
    }
}
