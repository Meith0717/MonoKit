// MonoKit - Camera2D.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.App.Input;

namespace MonoKit.Game.Camera;

public interface ICamera2DBehavior
{
    /// <summary>Called once when the behaviour is attached to the camera.</summary>
    void Initialize(Camera2D owner);

    /// <summary>Advances the behaviour each frame with input and elapsed time.</summary>
    void Update(Camera2D owner, InputHandler inputHandler, double elapsedGameTime);
}

public class Camera2D : ICamera
{
    private readonly List<ICamera2DBehavior> _behaviours = new();
    private readonly GraphicsDevice _graphicsDevice;

    public Vector2 Position = Vector2.Zero;
    public float ViewportZoom = 1;
    public float Zoom = 1;

    /// <summary>Creates the camera and initializes the view for the current viewport.</summary>
    public Camera2D(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        UpdateView(1);
    }

    public RectangleF Bounds { get; private set; }
    public Matrix ViewInvert { get; private set; }
    public Matrix View { get; private set; }

    /// <summary>Updates behaviours, then refreshes view, invert, and bounds.</summary>
    public void Update(double elapsedGameTime, InputHandler inputHandler)
    {
        _behaviours.ForEach(behaviour => behaviour.Update(this, inputHandler, elapsedGameTime));
        UpdateView(ViewportZoom);
    }

    Vector3 ICamera.Position
    {
        get => new(Position, 0);
        set => Position = new Vector2(value.X, value.Y);
    }

    /// <summary>Recomputes view matrices and world bounds for the viewport zoom.</summary>
    internal void UpdateView(float viewportScale)
    {
        var viewport = _graphicsDevice.Viewport.Bounds;
        ViewportZoom = viewportScale;
        View = CreateView(Position, Zoom * ViewportZoom, viewport.Width, viewport.Height);
        ViewInvert = Matrix.Invert(View);
        Bounds = TransformViewport(viewport, ViewInvert);
    }

    /// <summary>Initializes and registers a behaviour on this camera.</summary>
    public void AddBehaviour(ICamera2DBehavior behaviour)
    {
        behaviour.Initialize(this);
        _behaviours.Add(behaviour);
    }

    /// <summary>Builds the camera view from position, zoom, and viewport size.</summary>
    private static Matrix CreateView(
        Vector2 cameraPosition,
        float cameraZoom,
        int screenWidth,
        int screenHeight
    )
    {
        var translationMatrix = Matrix.CreateTranslation(
            new Vector3(-cameraPosition.X, -cameraPosition.Y, 0)
        );
        var scaleMatrix = Matrix.CreateScale(cameraZoom, cameraZoom, 1);
        var screenCenterMatrix = Matrix.CreateTranslation(
            new Vector3(screenWidth / 2f, screenHeight / 2f, 0)
        );

        return translationMatrix * scaleMatrix * screenCenterMatrix;
    }

    /// <summary>Transforms a screen rectangle into world-space bounds.</summary>
    private static RectangleF TransformViewport(Rectangle screenRect, Matrix cameraToWorld)
    {
        var topLeft = Vector2.Transform(screenRect.Location.ToVector2(), cameraToWorld);
        var bottomRight = Vector2.Transform(
            new Vector2(screenRect.Right, screenRect.Bottom),
            cameraToWorld
        );

        return new RectangleF(
            topLeft.X,
            topLeft.Y,
            bottomRight.X - topLeft.X,
            bottomRight.Y - topLeft.Y
        );
    }
}
