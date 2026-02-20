// ContentProvider.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;

namespace MonoKit.Content;

/// <summary>
///     Provides access to all loaded content containers.
/// </summary>
public sealed class ContentProvider
{
    private static readonly Lazy<ContentProvider> Instance = new(() => new ContentProvider());
    private readonly Dictionary<Type, object> _containers = new();

    /// <summary>
    ///     Registers a new content type container if it doesn’t exist.
    /// </summary>
    public static void Register<T>()
        where T : class
    {
        Instance.Value.RegisterInternal<T>();
    }

    /// <summary>
    ///     Gets the container for a given content type. Automatically registers it if missing.
    /// </summary>
    public static ContentContainer<T> Container<T>()
        where T : class
    {
        return Instance.Value.GetContainerInternal<T>();
    }

    public static T Get<T>(string id)
        where T : class
    {
        return Instance.Value.GetContainerInternal<T>().Get(id);
    }

    public static T Get<T>(int id)
        where T : class
    {
        return Instance.Value.GetContainerInternal<T>().Get(id);
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
