// GameServiceContainer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;

namespace GameEngine.Runtime
{
    public class GameServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new();

        public void AddService<T>(T service)
            => _services[typeof(T)] = service!;

        public T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new KeyNotFoundException($"Service of type {typeof(T).Name} not found.");
        }

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

}
