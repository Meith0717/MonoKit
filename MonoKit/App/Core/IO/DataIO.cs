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
    /// <summary>Populates an existing object from a JSON file on disk.</summary>
    public static TObj Load<TObj>(string path, TObj obj)
    {
        var jsonStr = FileUtils.ReadFile(path);
        obj = (TObj)JsonHandler.PopulateObject(obj, jsonStr);
        return obj;
    }

    /// <summary>Serializes an object to JSON and writes it to disk.</summary>
    public static void Save<TObj>(string path, TObj obj)
    {
        if (obj is null)
            throw new Exception();
        var jsonStr = JsonHandler.SerializeToJson(obj);
        FileUtils.CreateFile(path, jsonStr);
    }

    /// <summary>Loads an object on a background thread and invokes the callback.</summary>
    public static void LoadAsync<TObj>(string path, TObj newObj, Action<TObj> onComplete)
    {
        new Thread(_ =>
        {
            var obj = Load(path, newObj);
            onComplete?.Invoke(obj);
        }).Start();
    }

    /// <summary>Saves an object on a background thread and invokes the callback.</summary>
    public static void SaveAsync<TObj>(TObj obj, string path, Action onComplete)
    {
        new Thread(_ =>
        {
            Save(path, obj);
            onComplete?.Invoke();
        }).Start();
    }
}
