// MonoKit - JsonHandler.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Newtonsoft.Json;

namespace MonoKit.App.Core.IO;

internal static class JsonHandler
{
    /// <summary>Serializes an object to formatted JSON with type names.</summary>
    public static string SerializeToJson(object obj)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.Indented,
        };
        return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
    }

    /// <summary>Fills an existing object from JSON, replacing its members.</summary>
    public static object PopulateObject(object target, string jsonString)
    {
        if (jsonString is null)
            return target;
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Ignore,
        };
        JsonConvert.PopulateObject(jsonString, target, jsonSerializerSettings);
        return target;
    }
}
