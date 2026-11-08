// AppHost - RuntimeContainer.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;

namespace MonoKit.Game.Gameplay;

public class RuntimeContainer
{
    private readonly Dictionary<Type, object> _services = new();

    /// <summary>Registers a service instance under its type.</summary>
    public void AddService<T>(T service)
    {
        _services[typeof(T)] = service!;
    }

    /// <summary>Retrieves a service, throwing when it is not registered.</summary>
    public T Get<T>()
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;
        throw new KeyNotFoundException($"Service of type {typeof(T).Name} not found.");
    }

    /// <summary>Tries to retrieve a service; false when not registered.</summary>
    public bool TryGet<T>(out T value)
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            value = (T)service;
            return true;
        }

        value = default!;
        return false;
    }
}
