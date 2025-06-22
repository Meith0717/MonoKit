// Camera2d.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Engine.Gameplay;
using Engine.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Engine.Camera
{
    public class Camera2d(GraphicsDevice graphicsDevice)
    {
        public Vector2 Position = Vector2.Zero;
        public float Zoom = 1;
        public float Rotation = 0;

        private Rectangle _viewport;
        private readonly Renderer _renderer = new();
        private readonly GraphicsDevice _graphicsDevice = graphicsDevice;

        public RectangleF Frustum { get; private set; }

        public Matrix TransformationMatrix { get; private set; }

        public bool Contains(Vector2 position)
            => Frustum.Contains(position);

        public bool Intersects(CircleF circle)
            => Frustum.Intersects(circle);

        public bool Intersects(RectangleF rectangle)
            => Frustum.Intersects(rectangle);

        public void Update(SpatialHashing spatialHashing)
        {
            _viewport = _graphicsDevice.Viewport.Bounds;
            TransformationMatrix = CreateViewTransformationMatrix(Position, Zoom, Rotation, _viewport.Width, _viewport.Height);
            Frustum = TransformRectangle(_viewport, TransformationMatrix);
            _renderer.FrustumCulling(Frustum, spatialHashing);
        }

        public void Draw(SpriteBatch spriteBatch, GameRuntime scene)
        {
            _renderer.RenderCulledObjects(spriteBatch, TransformationMatrix, scene);
        }

        private static Matrix CreateViewTransformationMatrix(Vector2 cameraPosition, float cameraZoom, float cameraRotation, int screenWidth, int screenHeight)
        {
            Matrix translationMatrix = Matrix.CreateTranslation(new Vector3(-cameraPosition.X, -cameraPosition.Y, 0));
            Matrix rotationMatrix = Matrix.CreateRotationZ(cameraRotation);
            Matrix scaleMatrix = Matrix.CreateScale(cameraZoom, cameraZoom, 1);
            Matrix screenCenterMatrix = Matrix.CreateTranslation(new Vector3(screenWidth / 2f, screenHeight / 2f, 0));

            return translationMatrix * rotationMatrix * scaleMatrix * screenCenterMatrix;
        }

        private static RectangleF TransformRectangle(Rectangle worldBounds, Matrix ViewTransformationMatrix)
        {
            Point position = worldBounds.Location;
            int screenWidth = worldBounds.Width;
            int screenHeight = worldBounds.Height;

            Matrix inverse = Matrix.Invert(ViewTransformationMatrix);
            Vector2 LeftTopEdge = Vector2.Transform(position.ToVector2(), inverse);
            Vector2 RightBottomEdge = Vector2.Transform(new Vector2(screenWidth, screenHeight), inverse) - LeftTopEdge;
            return new(LeftTopEdge.X, LeftTopEdge.Y, RightBottomEdge.X, RightBottomEdge.Y);
        }
    }
}
