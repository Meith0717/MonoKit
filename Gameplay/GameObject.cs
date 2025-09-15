// GameObject.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace GameEngine.Gameplay
{
    [Serializable]
    public abstract class GameObject : IDisposable
    {
        [JsonProperty] private float _maxTextureSize;
        [JsonProperty] private CircleF _boundBox = new();

        // Physic Stuff
        [JsonIgnore] public CircleF BoundBox => _boundBox;
        [JsonProperty] public Vector2 MovingDirection = Vector2.One;
        [JsonProperty] public float Velocity;
        [JsonIgnore]
        public Vector2 Position
        {
            get => _boundBox.Position;
            set => _boundBox.Position = value;
        }
        [JsonIgnore]
        public float Scale
        {
            get => _boundBox.Radius * 2 / _maxTextureSize;
            set => _boundBox.Radius = _maxTextureSize / 2 * float.Max(0, value);
        }

        // Texture Stuff
        [JsonProperty] public string TextureId { get; private set; }
        [JsonProperty] public int RenderingDepth;
        [JsonProperty] public Color TextureColor = Color.White;

        // Managing Stuff
        [JsonIgnore] public bool IsDisposed { get; private set; }

        protected GameObject(Vector2 position, string textureId, float scale)
        {
            Position = position;
            TextureId = textureId;
            var texture = ContentProvider.Textures.Get(TextureId);
            _maxTextureSize = float.Max(texture.Width, texture.Height);
            Scale = scale;
        }

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        public abstract void Initialize(Runtime.RuntimeServiceContainer runtimeServices);

        public abstract void Update(double elapsedMs, Runtime.RuntimeServiceContainer runtimeServices);

        public abstract void Draw(SpriteBatch spriteBatch, Runtime.RuntimeServiceContainer runtimeServices);
    }
}
