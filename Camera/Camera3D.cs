// Camera3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameEngine.Camera
{
    public interface ICamera3dBehaviour
    {
        void Initiialize(Camera3D owner);
        public void Update(Camera3D owner, InputState inputState, double elapsedGameTime);
    }

    public class Camera3D(GraphicsDevice graphicsDevice)
    {
        private readonly List<ICamera3dBehaviour> _behaviours = new();

        public Vector3 Position;
        public Vector3 Forward;
        public Vector3 Up;
        public Vector3 Right;

        public float Fov = float.DegreesToRadians(60f);
        public float NearPlane = 0.1f;
        public float FarPlane = 1000f;
        public float AspectRatio = graphicsDevice.Viewport.AspectRatio;

        public Vector3 Target => Position + Forward;
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public void AddBehaviour(ICamera3dBehaviour behaviour)
        {
            behaviour.Initiialize(this);
            _behaviours.Add(behaviour);
        }

        public void Update(double elapsedGameTime, InputState inputState)
        {
            View = Matrix.CreateLookAt(Position, Target, Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(Fov, AspectRatio, NearPlane, FarPlane);
            _behaviours.ForEach(behaviour => behaviour.Update(this, inputState, elapsedGameTime));
        }
    }
}
