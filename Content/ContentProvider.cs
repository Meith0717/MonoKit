// ContentProvider.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoKit.Content
{
    public sealed class ContentProvider
    {
        private static ContentProvider Instance => _instance.Value;
        private static readonly Lazy<ContentProvider> _instance = new(() => new ContentProvider());

        public static ContentContainer<Texture2D> Textures => Instance._assetsInternal;
        public static ContentContainer<SpriteFont> Fonts => Instance._fontsInternal;
        public static ContentContainer<Effect> Effects => Instance._effectsInternal;
        public static ContentContainer<object> Objects => Instance._objectsInternal;

        private readonly ContentContainer<Texture2D> _assetsInternal;
        private readonly ContentContainer<SpriteFont> _fontsInternal;
        private readonly ContentContainer<Effect> _effectsInternal;
        private readonly ContentContainer<object> _objectsInternal;

        private ContentProvider()
        {
            _assetsInternal = new();
            _fontsInternal = new();
            _effectsInternal = new();
            _objectsInternal = new();
        }
    }
}
