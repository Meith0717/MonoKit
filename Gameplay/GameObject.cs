// GameObject.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Content;
using MonoKit.Spatial;
using Newtonsoft.Json;

namespace MonoKit.Gameplay;

[Serializable]
public abstract class GameObject : IDisposable, ISpatial
{
    [JsonProperty] private readonly float _maxTextureSize;
    [JsonProperty] private CircleF _boundBox;
    [JsonProperty] public Vector2 MovingDirection = Vector2.One;
    [JsonProperty] public int RenderingDepth;
    [JsonProperty] public Color TextureColor = Color.White;
    [JsonProperty] public float Velocity;

    protected GameObject(Vector2 position, string textureId, float scale)
    {
        Position = position;
        TextureId = textureId;
        var texture = ContentProvider.Container<Texture2D>().Get(TextureId);
        _maxTextureSize = float.Max(texture.Width, texture.Height);
        Scale = scale;
    }

    // Physic Stuff
    [JsonIgnore] public CircleF BoundBox => _boundBox;

    [JsonIgnore]
    public float Scale
    {
        get => _boundBox.Radius * 2 / _maxTextureSize;
        set => _boundBox.Radius = _maxTextureSize / 2 * float.Max(0, value);
    }

    // Texture Stuff
    [JsonProperty] public string TextureId { get; private set; }

    // Managing Stuff
    [JsonIgnore] public bool IsDisposed { get; private set; }

    public virtual void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    [JsonIgnore]
    public Vector2 Position
    {
        get => _boundBox.Position;
        set => _boundBox.Position = value;
    }

    public RectangleF Bounding => BoundBox.BoundingRectangle;

    public bool HasPositionChanged()
    {
        return Velocity != 0;
    }

    public abstract void Initialize(RuntimeContainer runtimeServices);

    public abstract void Update(double elapsedMs, RuntimeContainer runtimeServices);
}