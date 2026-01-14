// JsonHandler.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Newtonsoft.Json;

namespace MonoKit.Core.IO;

internal static class JsonHandler
{
    public static string SerializeToJson(object obj)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.Indented
        };
        return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
    }

    public static object PopulateObject(object target, string jsonString)
    {
        if (jsonString is null) return target;
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Ignore
        };
        JsonConvert.PopulateObject(jsonString, target, jsonSerializerSettings);
        return target;
    }
}