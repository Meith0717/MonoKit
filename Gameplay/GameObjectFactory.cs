// GameObjectFactory.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace GameEngine.Gameplay
{
    public abstract class GameObjectFactory<T>
    {
        protected readonly ContentContainer<T> ObjectPrefabs = new();
        public readonly ImmutableArray<string> IDs;

        public GameObjectFactory(ContentContainer<object> objectPrefabs)
        {
            var ids = new ConcurrentBag<string>();
            foreach (var kvp in objectPrefabs.Content)
            {
                if (kvp.Value is not T TData) continue;
                ObjectPrefabs.Add(kvp.Key, TData);
                ids.Add(kvp.Key);
            }
            ;
            IDs = ids.ToImmutableArray();
        }
    }
}
