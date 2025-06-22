// ContentContainer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Engine.Core;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Engine.Content
{
    public class ContentContainer<T>
    {
        private readonly Dictionary<string, T> _content = new();
        public string[] Loaded => _content.Keys.ToArray();

        public Dictionary<string, T> Content => _content;

        public T Get(string key)
        {
            if (_content.ContainsKey(key))
                return _content[key];
            if (typeof(T) == typeof(Texture2D))
                return _content["missingContent"];
            return default;
        }

        public void Add(string key, T value, ContentLoadingState contentLoadingState = null)
        {
            if (_content.ContainsKey(key))
                return;
            _content.Add(key, value);
        }

        public void LoadContent(ContentManager contentManager, string contentDirectory, ContentLoadingState contentLoadingState = null)
        {
            string fullContentDirectory = Path.Combine(contentManager.RootDirectory, contentDirectory);
            string[] files = FileHandler.GetAllFilesInDirectory(fullContentDirectory, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string directory = Path.GetDirectoryName(file);
                string contentId = Path.GetFileNameWithoutExtension(file);
                string relativePath = Path.GetRelativePath(contentManager.RootDirectory, directory);
                string contentPath = Path.Combine(relativePath, contentId);
                T content = contentManager.Load<T>(contentPath);
                Add(contentId, content);
                contentLoadingState?.AddLoaded(file);
            }
        }
    }

    public static class ContentContainerExtension
    {
        public static void LoadJsonDocuments(this ContentContainer<JsonDocument> contentContainer, ContentManager contentManager, string contentDirectory, ContentLoadingState contentLoadingState = null)
        {
            return;
            string fullContentDirectory = Path.Combine(contentManager.RootDirectory, contentDirectory);
            string[] files = FileHandler.GetAllFilesInDirectory(fullContentDirectory, SearchOption.AllDirectories);

            foreach (string prefabConfig in files)
            {
                string prefabConfigName = Path.GetFileNameWithoutExtension(prefabConfig);
                string jsonString = FileHandler.ReadFile(prefabConfig);
                var json = JsonDocument.Parse(jsonString);

                try
                {
                    json.RootElement.GetProperty("Type").ToString();
                    json.RootElement.GetProperty("Id").ToString();
                }
                catch
                {
                    throw new Exception($"Your json {prefabConfig} is missing an correct Type or Id property!");
                }

                contentContainer.Add(prefabConfigName, json);
                contentLoadingState?.AddLoaded(prefabConfig);
            }
        }
    }
}
