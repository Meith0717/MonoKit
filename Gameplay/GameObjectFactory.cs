// GameObjectFactory.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;
using System.Collections.Immutable;
using MonoKit.Content;

namespace MonoKit.Gameplay
{
    public abstract class GameObjectFactory<T>
    {
        public readonly ImmutableArray<byte> IDs;
        public readonly ImmutableArray<string> Names;
        public readonly ImmutableArray<T> Prefabs;

        public GameObjectFactory(ContentContainer<object> objectPrefabs)
        {
            var dataCount = objectPrefabs.Content.Count;
            
            byte i = 0;
            var ids = new List<byte>();
            var prefabs = new List<T>();
            var names = new List<string>();
             
            foreach (var kvp in objectPrefabs.Content)
            {
                if (kvp.Value is not T data) continue;
                
                names.Add(kvp.Key);
                ids.Add(i);
                prefabs.Add(data);
                i++;
            }
            
            IDs = [.. ids];
            Prefabs = [.. prefabs];
            Names = [.. names];
        }

        public T GetPrefab(byte id)
        {
            return id < Prefabs.Length ? Prefabs[id] : throw new KeyNotFoundException();
        }
        
        public T GetPrefab(string name)
        {
           var id = (byte)Names.IndexOf(name);
           if (int.IsNegative(id))
               throw new KeyNotFoundException();

           return GetPrefab(id);
        }
    }
}
