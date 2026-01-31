// ContentContainer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MonoKit.Core.IO;

namespace MonoKit.Content;

public class ContentContainer<T> : IEnumerable<(int, string, T)>
{
    private readonly List<T> _content = new();
    private readonly Dictionary<string, int> _nameToId = new();

    public IReadOnlyCollection<T> Loaded => _content.AsReadOnly();

    public T Get(string key)
    {
        if (_nameToId.TryGetValue(key, out var index))
            return _content[index];

        MessageBox.Show(
            "Missing content",
            $"'{key}' of type {typeof(T)}\ncold not be found.",
            ["ok"]
        );

        return default;
    }

    public T Get(int id) => id >= 0 && id < _content.Count ? _content[id] : default;

    public int GetId(string key) => _nameToId.GetValueOrDefault(key, -1);

    public void LoadContent(
        ContentManager contentManager,
        string contentDirectory,
        ContentLoadingState contentLoadingState = null,
        SearchOption searchOption = SearchOption.AllDirectories
    )
    {
        var fullContentDirectory = Path.Combine(contentManager.RootDirectory, contentDirectory);
        var files = FileUtils.GetAllFilesInDirectory(fullContentDirectory, searchOption);

        foreach (var file in files)
        {
            var directory = Path.GetDirectoryName(file);
            var contentId = Path.GetFileNameWithoutExtension(file);
            var relativePath = Path.GetRelativePath(contentManager.RootDirectory, directory!);
            var contentPath = Path.Combine(relativePath, contentId);
            var content = contentManager.Load<T>(contentPath);
            Add(contentId, content);
            contentLoadingState?.AddLoaded(file);
        }
    }

    public IEnumerator<(int, string, T)> GetEnumerator()
    {
        foreach (var (key, index) in _nameToId)
        {
            yield return (index, key, _content[index]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Add(string name, T content)
    {
        if (_nameToId.ContainsKey(name))
            return;

        var nextId = _content.Count;
        _nameToId.Add(name, nextId);
        _content.Add(content);
    }
}
