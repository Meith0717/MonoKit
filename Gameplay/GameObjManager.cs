// GameObjManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using MonoKit.Spatial;

namespace MonoKit.Gameplay;

public class GameObjManager(SpatialHashing spatialHashing, RuntimeContainer services)
{
    private readonly List<GameObject> _gameObjects = [];

    public IReadOnlyList<GameObject> GameObjects => _gameObjects.AsReadOnly();

    public void AddRange(GameObject[] gameObjects)
    {
        foreach (var obj in gameObjects)
        {
            obj.Initialize(services);
            _gameObjects.Add(obj);
            spatialHashing.Add(obj);
        }
    }

    public void Add(GameObject gameObject)
    {
        gameObject.Initialize(services);
        _gameObjects.Add(gameObject);
        spatialHashing.Add(gameObject);
    }

    public void Remove(GameObject gameObject)
    {
        _gameObjects.Remove(gameObject);
        spatialHashing.Remove(gameObject);
    }

    public void Update(double elapsedMs)
    {
        if (_gameObjects.Count == 0)
            return;

        foreach (var obj in _gameObjects)
        {
            obj.Update(elapsedMs, services);
            obj.Position += obj.MovingDirection * obj.Velocity * (float)elapsedMs;

            if (obj.IsDisposed)
                spatialHashing.Remove(obj);
        }

        _gameObjects.RemoveAll(obj => obj.IsDisposed);
    }
}
