// NewFileHandler.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using static System.Environment;

namespace MonoKit.Core
{
    public class PathManager<TPathId> where TPathId : Enum
    {
        public string RootPath { get; }
        private readonly Dictionary<TPathId, string> _paths = new();

        public PathManager(string rootPath = null, SpecialFolder? baseFolder = null)
        {
            string folderName = rootPath ?? "MonoKit";

            string basePath = baseFolder.HasValue
                ? Path.Combine(GetFolderPath(baseFolder.Value), folderName)
                : Path.GetFullPath(folderName);

            RootPath = basePath;
            FileUtils.CreateDirectory(RootPath);
        }

        public void RegisterPath(TPathId id, string relativePath)
        {
            string fullPath = Path.Combine(RootPath, relativePath);
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
}
