// Camera3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;
using System.Collections.Generic;

namespace MonoKit.Camera
{
    public interface ICamera3dBehaviour
    {
        void Initialize(Camera3D owner);
        public void Update(Camera3D owner, InputHandler inputHandler, double elapsedGameTime);
    }

    public class Camera3D(GraphicsDevice graphicsDevice)
    {
        private readonly List<ICamera3dBehaviour> _behaviours = new();

        public Vector3 Position = new(0f, 0f, 0f);
        public Vector3 Forward = new(0f, 0f, 1f);
        public Vector3 Up = Vector3.Up;
        public Vector3 Right = Vector3.Right;

        public float Fov = float.DegreesToRadians(60f);
        public float NearPlane = 0.1f;
        public float FarPlane = 1000f;
        public float AspectRatio = graphicsDevice.Viewport.AspectRatio;

        public Vector3 Target => Position + Forward;
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public void AddBehaviour(ICamera3dBehaviour behaviour)
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
