// MonoKit - ContentProvider.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;

namespace MonoKit.App.Content;

// ReSharper disable once ClassNeverInstantiated.Global
public class ContentProvider
{
    private readonly Dictionary<Type, object> _containers = new();

    public void Register<T>()
        where T : class
    {
        RegisterInternal<T>();
    }

    public ContentContainer<T> Container<T>()
        where T : class
    {
        return GetContainerInternal<T>();
    }

    public T Get<T>(string id)
        where T : class
    {
        return GetContainerInternal<T>().Get(id);
    }

    public T Get<T>(int id)
        where T : class
    {
        return GetContainerInternal<T>().Get(id);
    }

    private void RegisterInternal<T>()
        where T : class
    {
        if (!_containers.ContainsKey(typeof(T)))
            _containers[typeof(T)] = new ContentContainer<T>();
    }

    private ContentContainer<T> GetContainerInternal<T>()
        where T : class
    {
        if (_containers.TryGetValue(typeof(T), out var container))
            return (ContentContainer<T>)container;

        var newContainer = new ContentContainer<T>();
        _containers[typeof(T)] = newContainer;
        return newContainer;
    }
}
