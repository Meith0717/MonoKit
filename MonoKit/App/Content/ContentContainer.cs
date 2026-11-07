// AppHost - ContentContainer.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MonoKit.App.Content;

public class ContentContainer<T> : IEnumerable<(int, string, T)>
    where T : class
{
    private readonly List<T> _content = new();
    private readonly Dictionary<string, int> _nameToId = new();

    public IReadOnlyCollection<T> Content => _content.AsReadOnly();

    public IEnumerator<(int, string, T)> GetEnumerator()
    {
        foreach (var (key, index) in _nameToId)
            yield return (index, key, _content[index]);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public T Get(string key)
    {
        if (_nameToId.TryGetValue(key, out var index))
            return _content[index];

        MessageBox.Show(
            "Missing content",
            $"'{key}' of type {typeof(T)}\ncold not be found.",
            ["ok"]
        );

        return null;
    }

    public T Get(int id)
    {
        return id >= 0 && id < _content.Count ? _content[id] : default;
    }

    public int GetId(string key)
    {
        return _nameToId.GetValueOrDefault(key, -1);
    }

    public ContentContainer<T2> Cast<T2>()
        where T2 : class
    {
        var result = new ContentContainer<T2>();

        foreach (var (id, name, value) in this)
            if (value is T2 casted)
                result.Add(name, casted);

        return result;
    }

    public void Add(string name, T content)
    {
        if (_nameToId.ContainsKey(name))
            return;

        var nextId = _content.Count;
        _nameToId.Add(name, nextId);
        _content.Add(content);
    }
}
