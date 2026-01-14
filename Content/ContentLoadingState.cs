// ContentLoadingState.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using MonoKit.Core.IO;

namespace MonoKit.Content;

public class ContentLoadingState
{
    private readonly HashSet<string> _allContent;
    private readonly int _count;
    private readonly List<string> _loadedContent = new();

    public ContentLoadingState(ContentManager content)
    {
        _allContent =
        [
            .. FileUtils.GetAllFilesInDirectory(content.RootDirectory, SearchOption.AllDirectories),
        ];
        _count = _allContent.Count;
    }

    public double Progress => (double)_loadedContent.Count / _count;

    public string CurrentLoadedFile => _loadedContent.LastOrDefault(string.Empty);

    public void AddLoaded(string file)
    {
        if (!_allContent.Remove(file))
            throw new Exception("File does not exist");
        _loadedContent.Add(file);
    }
}
