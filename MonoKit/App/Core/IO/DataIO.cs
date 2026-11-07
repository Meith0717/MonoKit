// AppHost - DataIO.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Threading;

namespace MonoKit.App.Core.IO;

public static class DataIO
{
    public static TObj Load<TObj>(string path, TObj obj)
    {
        var jsonStr = FileUtils.ReadFile(path);
        obj = (TObj)JsonHandler.PopulateObject(obj, jsonStr);
        return obj;
    }

    public static void Save<TObj>(string path, TObj obj)
    {
        if (obj is null)
            throw new Exception();
        var jsonStr = JsonHandler.SerializeToJson(obj);
        FileUtils.CreateFile(path, jsonStr);
    }

    public static void LoadAsync<TObj>(string path, TObj newObj, Action<TObj> onComplete)
    {
        new Thread(_ =>
        {
            var obj = Load(path, newObj);
            onComplete?.Invoke(obj);
        }).Start();
    }

    public static void SaveAsync<TObj>(TObj obj, string path, Action onComplete)
    {
        new Thread(_ =>
        {
            Save(path, obj);
            onComplete?.Invoke();
        }).Start();
    }
}
