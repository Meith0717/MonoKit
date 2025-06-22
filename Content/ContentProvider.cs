// ContentProvider.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.Json;

namespace Engine.Content
{
    public sealed class ContentProvider
    {
        // Lazy-loaded singleton instance
        private static readonly Lazy<ContentProvider> _instance = new(() => new ContentProvider());
        private static ContentProvider Instance => _instance.Value;

        // Static proxy accessors
        public static ContentContainer<Texture2D> Textures => Instance._assetsInternal;
        public static ContentContainer<SpriteFont> Fonts => Instance._fontsInternal;
        public static ContentContainer<Effect> Effects => Instance._effectsInternal;
        public static ContentContainer<JsonDocument> Configs => Instance._configsInternal;

        // Internal content providers
        private readonly ContentContainer<Texture2D> _assetsInternal;
        private readonly ContentContainer<SpriteFont> _fontsInternal;
        private readonly ContentContainer<Effect> _effectsInternal;
        private readonly ContentContainer<JsonDocument> _configsInternal;

        // Private constructor
        private ContentProvider()
        {
            _assetsInternal = new();
            _fontsInternal = new();
            _effectsInternal = new();
            _configsInternal = new();
        }
    }
}
