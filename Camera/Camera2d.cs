// Camera2d.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Input;
using System.Collections.Generic;

namespace MonoKit.Camera
{
    public interface ICamera2dBehavior
    {
        void Initialize(Camera2D owner);
        void Update(Camera2D owner, InputHandler inputHandler, double elapsedGameTime);
    }

    public class Camera2D
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly List<ICamera2dBehavior> _behaviours = new();

        public Vector2 Position = Vector2.Zero;
        public float ViewportZoom = 1;
        public float Zoom = 1;

        public RectangleF Bounds { get; private set; }
        public Matrix View { get; private set; }
        public Matrix ViewInvert { get; private set; }

        public Camera2D(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            UpdateView(1);
        }

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
            Matrix translationMatrix = Matrix.CreateTranslation(new Vector3(-cameraPosition.X, -cameraPosition.Y, 0));
            Matrix scaleMatrix = Matrix.CreateScale(cameraZoom, cameraZoom, 1);
            Matrix screenCenterMatrix = Matrix.CreateTranslation(new Vector3(screenWidth / 2f, screenHeight / 2f, 0));

            return translationMatrix * scaleMatrix * screenCenterMatrix;
        }

        private static RectangleF TransformViewport(Rectangle screenRect, Matrix cameraToWorld)
        {
            Vector2 topLeft = Vector2.Transform(screenRect.Location.ToVector2(), cameraToWorld);
            Vector2 bottomRight = Vector2.Transform(new Vector2(screenRect.Right, screenRect.Bottom), cameraToWorld);

            return new RectangleF(
                topLeft.X,
                topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y);
        }
    }
}
