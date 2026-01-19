// GameObjectFactory.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Collections.Immutable;
using MonoKit.Content;

namespace MonoKit.Gameplay;

public abstract class GameObjectFactory<T>
{
    private readonly Dictionary<string, byte> _namesIdDictionary;
    protected readonly byte MaxID;
    public readonly ImmutableArray<T> Prefabs;

    public GameObjectFactory(ContentContainer<object> objectPrefabs)
    {
        byte id = 0;
        var prefabs = new List<T>();
        _namesIdDictionary = new Dictionary<string, byte>();

        foreach (var kvp in objectPrefabs.Content)
        {
            if (kvp.Value is not T data)
                continue;

            _namesIdDictionary.Add(kvp.Key, id);
            prefabs.Add(data);
            id++;
        }

        MaxID = id;
        Prefabs = [.. prefabs];
    }

    public T GetPrefab(byte id)
    {
        return id < Prefabs.Length ? Prefabs[id] : throw new KeyNotFoundException();
    }

    public T GetPrefab(string name)
    {
        if (_namesIdDictionary.TryGetValue(name, out var id))
            return GetPrefab(id);

        throw new KeyNotFoundException();
    }
}
