// Camera3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;

namespace MonoKit.Graphics.Camera
{
    public interface ICamera3dBehavior
    {
        void Initialize(Camera3D owner);
        void Update(Camera3D owner, InputHandler inputHandler, double elapsedGameTime);
    }

    public class Camera3D(Vector3 positon, GraphicsDevice graphicsDevice)
    {
        private readonly List<ICamera3dBehavior> _behaviours = new();

        public Vector3 Position = positon;
        public Vector3 Forward = new(1, 0, 0);
        public Vector3 Up = Vector3.Up;
        public Vector3 Right = Vector3.Right;

        public float Fov = float.DegreesToRadians(60f);
        public float NearPlane = 0.1f;
        public float FarPlane = 1000f;
        public float AspectRatio = graphicsDevice.Viewport.AspectRatio;

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

        public void AddBehaviour(ICamera3dBehavior behaviour)
        {
            behaviour.Initialize(this);
            _behaviours.Add(behaviour);
        }

        public void Update(double elapsedGameTime, InputHandler inputHandler)
        {
            View = Matrix.CreateLookAt(Position, Target, Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(Fov, AspectRatio, NearPlane, FarPlane);
            _behaviours.ForEach(behaviour => behaviour.Update(this, inputHandler, elapsedGameTime));
        }
    }
}
