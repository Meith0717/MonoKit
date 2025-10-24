// DataIO.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Threading;

namespace MonoKit.Core
{
    public static class DataIO
    {
        public static TObj Load<TObj>(string path, TObj obj)
        {
            string jsonStr = FileUtils.ReadFile(path);
            obj = (TObj)JsonHandler.PopulateObject(obj, jsonStr);
            return obj;
        }

        public static void Save<TObj>(string path, TObj obj)
        {
            if (obj is null) throw new Exception();
            string jsonStr = JsonHandler.SerializeToJson(obj);
            FileUtils.CreateFile(path, jsonStr);
        }

        public static void LoadAsync<TObj>(string path, TObj newObj, Action<TObj> onComplete)
            => new Thread(_ =>
            {
                var obj = Load(path, newObj);
                onComplete?.Invoke(obj);
            }).Start();

        public static void SaveAsync<TObj>(TObj obj, string path, Action onComplete)
            => new Thread(_ =>
            {
                Save(path, obj);
                onComplete?.Invoke();
            }).Start();
    }
}