// MoveByMouseBehaviour.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.Camera
{
    public class MoveByMouse
    {
        public class In2D(float smooth) : ICamera2dBehaviour
        {
            private readonly float _smooth = smooth;
            private Vector2 _positionTarget;
            private Vector2 _lastMousePosition;

            public void Initialize(Camera2D owner)
            {
                _positionTarget = owner.Position;
            }

            public void MoveToPosition(Vector2 target)
            {
                _positionTarget = target;
            }

            public void Update(Camera2D owner, InputState inputState, double elapsedGameTime)
            {
                if (inputState.HasAction(ActionType.RightClickHold))
                    _positionTarget += (_lastMousePosition - inputState.MousePosition) / (owner.Zoom * owner.ViewportZoom);

                _lastMousePosition = inputState.MousePosition;

                if (_positionTarget != owner.Position)
                {
                    Vector2 vectorDif = _positionTarget - owner.Position;
                    owner.Position += vectorDif * _smooth;
                }
            }
        }

        public class In3D(GraphicsDevice graphicsDevice) : ICamera3dBehaviour
        {
            private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
            private MouseState _prevMouseState;
            private float _yaw;
            private float _pitch;

            public void Initialize(Camera3D owner) {; }


            public void Update(Camera3D owner, InputState inputState, double elapsedGameTime)
            {
                KeyboardState ks = Keyboard.GetState();
                var mouseState = Mouse.GetState();

                float sensitivity = 0.002f;
                int deltaX = mouseState.X - _prevMouseState.X;
                int deltaY = mouseState.Y - _prevMouseState.Y;
                _yaw -= deltaX * sensitivity;
                _pitch -= deltaY * sensitivity;

                _pitch = float.Clamp(_pitch, -float.Pi / 2 + 0.01f, float.Pi / 2 - 0.01f);

                var x = float.Cos(_pitch) * float.Sin(_yaw);
                var y = float.Sin(_pitch);
                var z = float.Cos(_pitch) * float.Cos(_yaw);
                owner.Forward = new(x, y, z);
                owner.Right = Vector3.Normalize(Vector3.Cross(owner.Forward, Vector3.Up));
                owner.Up = Vector3.Cross(owner.Right, owner.Forward);

                float movingSpeed = .01f * (float)elapsedGameTime;
                if (ks.IsKeyDown(Keys.W))
                    owner.Position += owner.Forward * movingSpeed;
                if (ks.IsKeyDown(Keys.S))
                    owner.Position -= owner.Forward * movingSpeed;
                if (ks.IsKeyDown(Keys.A))
                    owner.Position -= owner.Right * movingSpeed;
                if (ks.IsKeyDown(Keys.D))
                    owner.Position += owner.Right * movingSpeed;
                if (ks.IsKeyDown(Keys.PageUp))
                    owner.Position += owner.Up * movingSpeed;
                if (ks.IsKeyDown(Keys.PageDown))
                    owner.Position -= owner.Up * movingSpeed;

                Mouse.SetPosition(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);
                _prevMouseState = Mouse.GetState();
            }
        }
    }
}
