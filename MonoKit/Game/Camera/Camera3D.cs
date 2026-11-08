// AppHost - Camera3D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Input;

namespace MonoKit.Game.Camera;

public interface ICamera3DBehavior
{
    /// <summary>Called once when the behaviour is attached to the camera.</summary>
    void Initialize(Camera3D owner);
    /// <summary>Advances the behaviour each frame with input and elapsed time.</summary>
    void Update(Camera3D owner, InputHandler inputHandler, double elapsedGameTime);
}

public class Camera3D(Vector3 positon, GraphicsDevice graphicsDevice) : ICamera
{
    private readonly List<ICamera3DBehavior> _behaviours = new();
    public float FarPlane = 1000f;
    public Vector3 Forward = new(1, 0, 0);

    public float Fov = float.DegreesToRadians(60f);
    public float NearPlane = 0.1f;

    public Vector3 Position = positon;

    Vector3 ICamera.Position
    {
        get => Position;
        set => Position = value;
    }
    public Vector3 Right = Vector3.Right;
    public Vector3 Up = Vector3.Up;

    public Vector3 Target
    {
        get => Position + Forward;
        set
        {
            Forward = Vector3.Normalize(value - Position);
            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.Up));
            Up = Vector3.Cross(Right, Forward);
        }
    }

    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    /// <summary>Initializes and registers a behaviour on this camera.</summary>
    public void AddBehaviour(ICamera3DBehavior behaviour)
    {
        behaviour.Initialize(this);
        _behaviours.Add(behaviour);
    }

    /// <summary>Recomputes view and projection, then updates behaviours.</summary>
    public void Update(double elapsedGameTime, InputHandler inputHandler)
    {
        View = Matrix.CreateLookAt(Position, Target, Up);
        Projection = Matrix.CreatePerspectiveFieldOfView(
            Fov,
            graphicsDevice.Viewport.AspectRatio,
            NearPlane,
            FarPlane
        );
        _behaviours.ForEach(behaviour => behaviour.Update(this, inputHandler, elapsedGameTime));
    }
}
