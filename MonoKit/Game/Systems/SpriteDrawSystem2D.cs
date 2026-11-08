// SpriteDrawSystem2D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;
using MonoKit.Game.Camera;
using MonoKit.Game.Components;

namespace MonoKit.Game.Systems;

/// <summary>Draws all Transform2D+SpriteRenderer entities through the 2D camera, centered on <see cref="Camera2D.Position"/>.</summary>
public class SpriteDrawSystem2D(Camera2D camera) : IDrawSystem
{
    private readonly Camera2D _camera = camera;

    public void Initialize(World world) { }

    public void Draw(double elapsedMs, World world, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        spriteBatch.Begin(transformMatrix: _camera.View, sortMode: SpriteSortMode.BackToFront);
        world.ForEach<Transform2D, SpriteRenderer>(
            (Entity e, ref Transform2D t, ref SpriteRenderer s) =>
            {
                var origin = new Vector2(s.TextureWidth, s.TextureHeight) / 2f;
                spriteBatch.Draw(
                    s.Texture,
                    t.Position,
                    null,
                    s.Color == default ? Color.White : s.Color,
                    t.Rotation,
                    origin,
                    s.TextureScale,
                    SpriteEffects.None,
                    s.LayerDepth
                );
            }
        );
        spriteBatch.End();
    }

    public void Dispose() { }
}
