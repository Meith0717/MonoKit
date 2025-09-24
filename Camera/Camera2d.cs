// Camera2d.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace GameEngine.Camera
{
    public class Camera2d
    {
        private readonly GraphicsDevice _graphicsDevice;

        public Vector2 Position = Vector2.Zero;
        public float Zoom = 1;

        public Camera2d(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            Update(1);
        }

        public RectangleF Bounds { get; private set; }

        public Matrix WorldToCamera { get; private set; }

        public Matrix CameraToWorld { get; private set; }

        public void Update(float viewportScale)
        {
            var viewport = _graphicsDevice.Viewport.Bounds;
            WorldToCamera = CreateViewTransformationMatrix(Position, Zoom * viewportScale, viewport.Width, viewport.Height);
            CameraToWorld = Matrix.Invert(WorldToCamera);
            Bounds = TransformRectangle(viewport, CameraToWorld);
        }

        private static Matrix CreateViewTransformationMatrix(Vector2 cameraPosition, float cameraZoom, int screenWidth, int screenHeight)
        {
            Matrix translationMatrix = Matrix.CreateTranslation(new Vector3(-cameraPosition.X, -cameraPosition.Y, 0));
            Matrix scaleMatrix = Matrix.CreateScale(cameraZoom, cameraZoom, 1);
            Matrix screenCenterMatrix = Matrix.CreateTranslation(new Vector3(screenWidth / 2f, screenHeight / 2f, 0));

            return translationMatrix * scaleMatrix * screenCenterMatrix;
        }

        private static RectangleF TransformRectangle(Rectangle screenRect, Matrix cameraToWorld)
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
