// JsonHandler.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json;

namespace MonoKit.Core
{
    internal static class JsonHandler
    {
        public static Dictionary<string, JsonElement> LoadJsonInDictionary(string jsonString)
            => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

        public static string SerializeToJson(object obj)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
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
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
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
}
