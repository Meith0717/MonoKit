// GameObjManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;

namespace MonoKit.Gameplay
{
    public class GameObjManager(SpatialHashing spatialHashing, RuntimeContainer services)
    {
        private readonly SpatialHashing _spatialHashing = spatialHashing;
        private readonly RuntimeContainer _services = services;
        private readonly List<GameObject> _gameObjects = new();

        public IReadOnlyList<GameObject> GameObjects => _gameObjects.AsReadOnly();

        public void AddRange(GameObject[] gameObjects)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                var obj = gameObjects[i];
                obj.Initialize(_services);
                _gameObjects.Add(obj);
                _spatialHashing.Add(obj);
            }
        }

        public void Add(GameObject gameObject)
        {
            gameObject.Initialize(_services);
            _gameObjects.Add(gameObject);
            _spatialHashing.Add(gameObject);
        }

        public void Remove(GameObject gameObject)
        {
            _gameObjects.Remove(gameObject);
            _spatialHashing.Remove(gameObject);
        }

        public void Update(double elapsedMs)
        {
            if (_gameObjects is null || _gameObjects.Count == 0) return;

            for (var i = 0; i < _gameObjects.Count; i++)
            {
                var obj = _gameObjects[i];

                obj.Update(elapsedMs, _services);
                obj.Position += obj.MovingDirection * obj.Velocity * (float)elapsedMs;

                if (obj.IsDisposed)
                    _spatialHashing.Remove(obj);
            }

            _gameObjects.RemoveAll(obj => obj.IsDisposed);
        }
    }
}
