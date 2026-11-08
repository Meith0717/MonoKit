// GameplayScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App;
using MonoKit.App.Input;
using MonoKit.App.Ui;
using MonoKit.Game;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.Components;
using MonoKit.Game.Screens;

namespace MonoKit.Demo.Screens;

/// <summary>
///     2D gameplay on the ECS runtime: a player ship moved with WASD/arrows, Space to shoot
///     (bullets get a Lifetime, asteroids spawn endlessly and expire), the camera follows the
///     ship, and P opens a pause overlay stacked on top (game keeps drawing below, stops updating).
/// </summary>
public class GameplayScreen(AppHost appHost, GameServiceContainer services)
    : GameScreen(services, RuntimeDimension.TwoD)
{
    private const float ShipSpeed = 0.25f;
    private const float BulletSpeed = 0.6f;
    private const double ShootCooldownMs = 200;
    private const double BulletLifetimeMs = 1500;
    private const double AsteroidSpawnMs = 900;
    private const double AsteroidLifetimeMs = 10000;

    private Entity _ship = default;
    private UiText _fps = null!;
    private UiText _score = null!;
    private double _elapsedSinceShot;
    private double _elapsedSinceSpawn;
    private int _shots;
    private readonly Random _random = new();

    public override void Initialize()
    {
        var font = appHost.ContentProvider.Get<SpriteFont>("default");
        var shipTex = appHost.ContentProvider.Get<Texture2D>("player");

        _ship = Host.World.CreateEntityWithComponents(
            new Transform2D { Position = Vector2.Zero, Rotation = 0 },
            new Velocity2D
            {
                Velocity = 0,
                NormalizedMovingDirection = Vector2.Zero,
                AngularVelocity = 0,
            },
            new SpriteRenderer(shipTex) { TextureScale = 0.35f }
        );

        // HUD
        _fps = new UiText(font)
        {
            TextProvider = () => $"{appHost.FrameCounter.CurrentFramesPerSecond:F0} fps",
            Align = Align.NW,
            Color = Color.White,
        };
        _score = new UiText(font)
        {
            TextProvider = () => $"Shots: {_shots}",
            Align = Align.NE,
            Color = Color.White,
        };
    }

    public override void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        if (inputHandler.HasAction((byte)DemoAction.Back))
        {
            appHost.ScreenManager.PopScreen();
            return;
        }

        if (inputHandler.HasAction((byte)DemoAction.Pause))
        {
            appHost.ScreenManager.AddScreen(new PauseMenuScreen(appHost, services));
            return;
        }

        var dir = Vector2.Zero;
        if (inputHandler.IsActionActive((byte)DemoAction.Up))
            dir.Y -= 1;
        if (inputHandler.IsActionActive((byte)DemoAction.Down))
            dir.Y += 1;
        if (inputHandler.IsActionActive((byte)DemoAction.Left))
            dir.X -= 1;
        if (inputHandler.IsActionActive((byte)DemoAction.Right))
            dir.X += 1;

        ref var velocity = ref Host.World.GetComponent<Velocity2D>(_ship);
        ref var transform = ref Host.World.GetComponent<Transform2D>(_ship);

        if (dir != Vector2.Zero)
        {
            velocity.NormalizedMovingDirection = Vector2.Normalize(dir);
            velocity.Velocity = ShipSpeed;
            transform.Rotation = MathF.Atan2(dir.Y, dir.X);
        }
        else
        {
            velocity.Velocity = 0;
        }

        // Shooting (cooldown-gated): a bullet entity with a lifetime.
        _elapsedSinceShot += elapsedMs;
        if (
            inputHandler.IsActionActive((byte)DemoAction.Shoot)
            && _elapsedSinceShot >= ShootCooldownMs
        )
        {
            _elapsedSinceShot = 0;
            _shots++;
            var bulletTex = appHost.ContentProvider.Get<Texture2D>("pickup");
            Host.World.CreateEntityWithComponents(
                new Transform2D { Position = transform.Position, Rotation = 0 },
                new Velocity2D
                {
                    Velocity = BulletSpeed,
                    NormalizedMovingDirection = -Vector2.UnitY,
                    AngularVelocity = 0,
                },
                new Lifetime { CoolDown = BulletLifetimeMs, DestroyNow = false },
                new SpriteRenderer(bulletTex) { TextureScale = 0.1f }
            );
        }

        // Asteroid field: spawn around the player, drift, expire via Lifetime.
        _elapsedSinceSpawn += elapsedMs;
        if (_elapsedSinceSpawn >= AsteroidSpawnMs)
        {
            _elapsedSinceSpawn = 0;
            var asteroidTex = appHost.ContentProvider.Get<Texture2D>("asteroid");
            var angle = _random.NextDouble() * Math.PI * 2;
            var distance = 150f + _random.NextSingle() * 350f;
            var spawnPosition =
                transform.Position
                + new Vector2(MathF.Cos((float)angle), MathF.Sin((float)angle)) * distance;
            var headingAngle = _random.NextDouble() * Math.PI * 2;
            Host.World.CreateEntityWithComponents(
                new Transform2D { Position = spawnPosition, Rotation = (float)headingAngle },
                new Velocity2D
                {
                    Velocity = 0.05f + _random.NextSingle() * 0.05f,
                    NormalizedMovingDirection = new Vector2(
                        MathF.Cos((float)headingAngle),
                        MathF.Sin((float)headingAngle)
                    ),
                    AngularVelocity = 0,
                },
                new Lifetime { CoolDown = AsteroidLifetimeMs, DestroyNow = false },
                new SpriteRenderer(asteroidTex)
                {
                    TextureScale = 0.25f + _random.NextSingle() * 0.2f,
                }
            );
        }

        base.Update(elapsedMs, inputHandler, uiScale);
        Host.Camera2D!.Position = Host.World.GetComponent<Transform2D>(_ship).Position;
    }

    public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        base.Draw(spriteBatch, graphicsDevice); // world through the camera

        // HUD in screen space.
        spriteBatch.Begin();
        var bounds = graphicsDevice.Viewport.Bounds;
        _fps.Update(appHost.InputHandler, bounds, uiScale: 1f);
        _score.Update(appHost.InputHandler, bounds, uiScale: 1f);
        _fps.Draw(spriteBatch, graphicsDevice);
        _score.Draw(spriteBatch, graphicsDevice);
        spriteBatch.End();
    }
}
