// MonoKit - GameHost.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Input;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.Camera;
using MonoKit.Game.Gameplay;

namespace MonoKit.Game;

/// <summary>Selects the component/system set of a <see cref="GameHost" />.</summary>
public enum RuntimeDimension
{
    TwoD,
    ThreeD,
}

/// <summary>
///     Unified game runtime: an AECS world + camera, configured for 2D or 3D via
///     <see cref="RuntimeDimension" />. Update drives world+camera, Draw runs the world's
///     IDrawSystem pass.
/// </summary>
public class GameHost : IDisposable
{
    public readonly RuntimeContainer Services = new();

    /// <summary>Builds the world, camera, and services for the chosen dimension.</summary>
    public GameHost(GraphicsDevice graphicsDevice, RuntimeDimension dimension)
    {
        Dimension = dimension;

        Camera = dimension switch
        {
            RuntimeDimension.TwoD => new Camera2D(graphicsDevice),
            RuntimeDimension.ThreeD => new Camera3D(Vector3.Zero, graphicsDevice),
            _ => throw new NotImplementedException(),
        };

        Services.AddService(Camera);
        World = new WorldBuilder().Build();
        Services.AddService(World);
    }

    public World World { get; }
    public RuntimeDimension Dimension { get; }

    /// <summary>The active camera, dimension-agnostic.</summary>
    public ICamera Camera { get; }

    /// <summary>Typed 2D camera when <see cref="Dimension" /> is TwoD, else null. For 2D-only extras (Zoom, ViewInvert).</summary>
    public Camera2D? Camera2D => Camera as Camera2D;

    /// <summary>Typed 3D camera when <see cref="Dimension" /> is ThreeD, else null. For 3D-only extras (Projection, Target).</summary>
    public Camera3D? Camera3D => Camera as Camera3D;

    public double LastElapsedMs { get; private set; }
    public Vector2 WorldMousePosition { get; private set; }

    /// <summary>Disposes the underlying world.</summary>
    public void Dispose()
    {
        World.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Advances the world and camera; refreshes the world mouse position.</summary>
    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        LastElapsedMs = elapsedMilliseconds;
        World.Update(elapsedMilliseconds);

        Camera.Update(elapsedMilliseconds, inputHandler);

        if (Dimension == RuntimeDimension.TwoD && Camera2D != null)
            WorldMousePosition = Vector2.Transform(
                Mouse.GetState().Position.ToVector2(),
                Camera2D.ViewInvert
            );
    }

    /// <summary>Runs the world's draw systems (e.g. SpriteDrawSystem2D) with the given batch/device.</summary>
    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        World.Draw(LastElapsedMs, spriteBatch, graphicsDevice);
    }
}
