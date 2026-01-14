// ContentProvider.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;

namespace MonoKit.Content;

/// <summary>
///     Provides access to all loaded content containers.
/// </summary>
public sealed class ContentProvider
{
    private static readonly Lazy<ContentProvider> _instance = new(() => new ContentProvider());

    private readonly Dictionary<Type, object> _containers = new();
    private static ContentProvider Instance => _instance.Value;

    /// <summary>
    ///     Registers a new content type container if it doesn’t exist.
    /// </summary>
    public static void Register<T>()
    {
        Instance.RegisterInternal<T>();
    }

    /// <summary>
    ///     Gets the container for a given content type. Automatically registers it if missing.
    /// </summary>
    public static ContentContainer<T> Container<T>()
    {
        return Instance.GetContainerInternal<T>();
    }

    public static T Get<T>(string id)
    {
        return Instance.GetContainerInternal<T>().Get(id);
    }

    private void RegisterInternal<T>()
    {
        if (!_containers.ContainsKey(typeof(T)))
            _containers[typeof(T)] = new ContentContainer<T>();
    }

    private ContentContainer<T> GetContainerInternal<T>()
    {
        if (_containers.TryGetValue(typeof(T), out var container))
            return (ContentContainer<T>)container;

        var newContainer = new ContentContainer<T>();
        _containers[typeof(T)] = newContainer;
        return newContainer;
    }
}