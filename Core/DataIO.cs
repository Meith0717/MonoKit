// DataIO.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Threading;

namespace Engine.Core
{
    public static class DataIO
    {
        public static Obj Load<Obj>(string path, Obj obj)
        {
            string jsonStr = FileHandler.ReadFile(path);
            obj = (Obj)JsonHandler.PopulateObject(obj, jsonStr);
            return obj;
        }

        public static void Save<Obj>(Obj obj, string path, Action onComplete)
        {
            if (obj is null) throw new Exception();
            string jsonStr = JsonHandler.SerializeToJson(obj);
            FileHandler.CreateFile(path, jsonStr);
            onComplete?.Invoke();
        }

        public static void LoadAsync<Obj>(string path, Obj newObj, Action<Obj> onComplete)
            => new Thread(_ =>
            {
                var obj = Load(path, newObj);
                onComplete.Invoke(obj);
            }).Start();

        public static void SaveAsync<Obj>(Obj obj, string path, Action onComplete)
            => new Thread(_ => Save(obj, path, onComplete)).Start();
    }
}