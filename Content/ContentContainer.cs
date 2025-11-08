// ContentContainer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Core;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace MonoKit.Content
{
    public class ContentContainer<T>
    {
        private readonly Dictionary<string, T> _content = new();
        public string[] Loaded => _content.Keys.ToArray();

        public ImmutableDictionary<string, T> Content => _content.ToImmutableDictionary();

        public T Get(string key)
        {
            if (_content.ContainsKey(key))
                return _content[key];
            MessageBox.Show("Missing content", $"'{key}' of type {typeof(T)}\ncold not be found.", ["ok"]);
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

        public void LoadContent(ContentManager contentManager, string contentDirectory, ContentLoadingState contentLoadingState = null, SearchOption searchOption = SearchOption.AllDirectories)
        {
            string fullContentDirectory = Path.Combine(contentManager.RootDirectory, contentDirectory);
            string[] files = FileUtils.GetAllFilesInDirectory(fullContentDirectory, searchOption);

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
}
