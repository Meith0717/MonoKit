// GameObjectFactory.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GameEngine.Gameplay
{
    public abstract class GameObjectFactory<T>
    {
        private readonly Dictionary<byte, T> _objectPrefabs = new();
        public readonly ImmutableArray<byte> IDs;
        public readonly ImmutableArray<T> Prefabs;

        public GameObjectFactory(ContentContainer<object> objectPrefabs)
        {
            byte i = 0;
            var ids = new List<byte>();
            foreach (var kvp in objectPrefabs.Content)
            {
                if (kvp.Value is not T TData) continue;
                _objectPrefabs.Add(i, TData);
                ids.Add(i);
                i++;
            }
            IDs = [.. ids];
            Prefabs = [.. _objectPrefabs.Values];
        }

        public T GetPrefab(byte id) 
        {
            if (_objectPrefabs.TryGetValue(id, out var value))
                return value;
            throw new KeyNotFoundException();
        }

    }
}
