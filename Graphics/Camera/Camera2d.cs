// Camera2d.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Input;

namespace MonoKit.Graphics.Camera;

public interface ICamera2dBehavior
{
    void Initialize(Camera2D owner);
    void Update(Camera2D owner, InputHandler inputHandler, double elapsedGameTime);
}

public class Camera2D
{
    private readonly List<ICamera2dBehavior> _behaviours = new();
    private readonly GraphicsDevice _graphicsDevice;

    public Vector2 Position = Vector2.Zero;
    public float ViewportZoom = 1;
    public float Zoom = 1;

    public Camera2D(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        UpdateView(1);
    }

    public RectangleF Bounds { get; private set; }
    public Matrix View { get; private set; }
    public Matrix ViewInvert { get; private set; }

    public void Update(double elapsedGameTime, InputHandler inputHandler)
    {
        _behaviours.ForEach(behaviour => behaviour.Update(this, inputHandler, elapsedGameTime));
    }

    public void UpdateView(float viewportScale)
    {
        var viewport = _graphicsDevice.Viewport.Bounds;
        ViewportZoom = viewportScale;
        View = CreateView(Position, Zoom * ViewportZoom, viewport.Width, viewport.Height);
        ViewInvert = Matrix.Invert(View);
        Bounds = TransformViewport(viewport, ViewInvert);
    }

    public void AddBehaviour(ICamera2dBehavior behaviour)
    {
        behaviour.Initialize(this);
        _behaviours.Add(behaviour);
    }

    private static Matrix CreateView(Vector2 cameraPosition, float cameraZoom, int screenWidth, int screenHeight)
    {
        var translationMatrix = Matrix.CreateTranslation(new Vector3(-cameraPosition.X, -cameraPosition.Y, 0));
        var scaleMatrix = Matrix.CreateScale(cameraZoom, cameraZoom, 1);
        var screenCenterMatrix = Matrix.CreateTranslation(new Vector3(screenWidth / 2f, screenHeight / 2f, 0));

        return translationMatrix * scaleMatrix * screenCenterMatrix;
    }

    private static RectangleF TransformViewport(Rectangle screenRect, Matrix cameraToWorld)
    {
        var topLeft = Vector2.Transform(screenRect.Location.ToVector2(), cameraToWorld);
        var bottomRight = Vector2.Transform(new Vector2(screenRect.Right, screenRect.Bottom), cameraToWorld);

        return new RectangleF(
            topLeft.X,
            topLeft.Y,
            bottomRight.X - topLeft.X,
            bottomRight.Y - topLeft.Y);
    }
}