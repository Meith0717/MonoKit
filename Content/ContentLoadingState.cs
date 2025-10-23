// ContentLoadingState.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Content;
using MonoKit.Core;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.Content
{
    public class ContentLoadingState
    {
        private readonly int _count;
        private readonly HashSet<string> _allContent;
        private readonly List<string> _loadedContent = new();

        public ContentLoadingState(ContentManager content)
        {
            _allContent = [.. FileHandler.GetAllFilesInDirectory(content.RootDirectory, System.IO.SearchOption.AllDirectories)];
            _count = _allContent.Count;
        }

        public void AddLoaded(string file)
        {
            if (!_allContent.Remove(file))
                throw new System.Exception("File does not exist");
            _loadedContent.Add(file);
        }

        public double Progress
            => (double)_loadedContent.Count / _count;

        public string CurrentLoadedFile
            => _loadedContent.LastOrDefault(string.Empty);
    }
}
