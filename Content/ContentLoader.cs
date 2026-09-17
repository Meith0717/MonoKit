// ContentLoader.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using MonoKit.Core.IO;

namespace MonoKit.Content;

public class ContentLoader(ContentManager contentManager)
{
    private readonly List<Func<ContentProvider, string>> _loadActions = [];

    private ContentProvider _contentProvider;
    private bool _isIncrementalMode = false;
    private int _loadedCount = 0;

    public ContentLoader RegisterContentDirectory<T>(
        string contentDirectory,
        SearchOption searchOption = SearchOption.AllDirectories
    )
        where T : class
    {
        var fullContentDirectory = Path.Combine(contentManager.RootDirectory, contentDirectory);
        if (!Directory.Exists(fullContentDirectory))
            throw new DirectoryNotFoundException(fullContentDirectory);

        var files = FileUtils.GetAllFilesInDirectory(fullContentDirectory, searchOption);
        foreach (var fileLocation in files)
        {
            var directory = Path.GetDirectoryName(fileLocation);
            var contentId = Path.GetFileNameWithoutExtension(fileLocation);
            var relativePath = Path.GetRelativePath(contentManager.RootDirectory, directory!);
            var contentPath = Path.Combine(relativePath, contentId);

            _loadActions.Add(cp =>
            {
                var content = contentManager.Load<T>(contentPath);
                var contentContainer = cp.Container<T>();
                contentContainer.Add(contentId, content);
                return contentPath;
            });
        }

        return this;
    }

    public void LoadAll(ContentProvider contentProvider)
    {
        if (_isIncrementalMode)
            throw new InvalidOperationException(
                "Cannot LoadAll after starting incremental loading. "
                    + "Use BeginIncremental()/LoadNext() or start fresh with a new loader."
            );

        _loadedCount = 0;
        while (_loadedCount < _loadActions.Count)
        {
            LoadNextInternal(contentProvider);
        }
    }

    public ContentLoader BeginIncremental(ContentProvider contentProvider)
    {
        if (_loadedCount > 0)
            throw new InvalidOperationException(
                "Cannot begin incremental loading after already loading some content. "
                    + "Create a new loader or Reset()."
            );

        _contentProvider = contentProvider;
        _isIncrementalMode = true;
        _loadedCount = 0;

        return this;
    }

    public (string LoadedFilePath, float Percentage) LoadNext()
    {
        if (!_isIncrementalMode)
            throw new InvalidOperationException("Must call BeginIncremental() before LoadNext().");

        var result = LoadNextInternal(_contentProvider);
        return (result.FilePath, result.Percentage);
    }

    public void EndIncremental()
    {
        if (!_isIncrementalMode || !IsComplete)
            throw new InvalidOperationException(
                "Cannot end incremental loading until all files are loaded."
            );
        _contentProvider = null;
        _isIncrementalMode = false;
    }

    public void Reset()
    {
        if (_isIncrementalMode)
            throw new InvalidOperationException(
                "Cannot Reset during incremental loading. Call EndIncremental() first."
            );
        _loadedCount = 0;
    }

    private (string FilePath, float Percentage) LoadNextInternal(ContentProvider contentProvider)
    {
        if (_loadedCount >= _loadActions.Count)
            return (null, 1f);

        var action = _loadActions[_loadedCount];
        var filePath = action(contentProvider);
        _loadedCount++;

        return (filePath, (float)_loadedCount / _loadActions.Count);
    }

    private bool IsComplete => _loadedCount >= _loadActions.Count;
}
