// GameScreen2D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.GameRuntime.ArchetypeEcs.Entities;
using MonoKit.GameRuntime.Components;
using MonoKit.GameRuntime.Gameplay;

namespace MonoKit.GameRuntime.Screens;

/// <summary>
/// Screen hosting a <see cref="GameRuntime2D"/>. Default draw renders all
/// Transform2D+SpriteRenderer entities through the 2D camera. Override
/// <see cref="Initialize"/> to spawn content and <see cref="Update"/> for input/game logic.
/// </summary>
public class GameScreen2D : IScreen
{
    public GameRuntime2D Runtime { get; }
    public bool UpdateBelow { get; }
    public bool DrawBelow { get; }

    public GameScreen2D(GameServiceContainer services, bool updateBelow = false, bool drawBelow = true)
    {
        Runtime = new GameRuntime2D(services.GetService<GraphicsDevice>());
        UpdateBelow = updateBelow;
        DrawBelow = drawBelow;
    }

    public virtual void Initialize() { }

    public virtual void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        Runtime.Update(elapsedMs, inputHandler);
    }

    public virtual void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        var cam = Runtime.Camera;
        var center = new Vector3(graphicsDevice.Viewport.Width / 2f, graphicsDevice.Viewport.Height / 2f, 0);
        var view = Matrix.CreateTranslation(new Vector3(-cam.Position, 0)) * Matrix.CreateTranslation(center);

        spriteBatch.Begin(transformMatrix: view, sortMode: SpriteSortMode.BackToFront);
        Runtime.World.ForEach<Transform2D, SpriteRenderer>(
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

    public virtual void OnResolutionChanged(float uiScale) { }

    public virtual void Dispose()
    {
        Runtime.Dispose();
    }
}
