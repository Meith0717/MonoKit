// CameraMover.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace GameEngine.Camera
{
    public class CameraMover(Camera2d camera)
    {
        private readonly Camera2d _camera = camera;
        private float _zoomTarget = camera.Zoom;
        private Vector2 _positionTarget = camera.Position;
        private Vector2 _lastMousePosition = Vector2.Zero;
        private float _elapsedMilliseconds;

        public void Update(double elapsedMilliseconds)
        {
            _elapsedMilliseconds = (float)elapsedMilliseconds;
            float smothFactor = 100 / _elapsedMilliseconds;
            UpdateZoom(smothFactor);
            UpdatePositoin(smothFactor);
        }

        private void UpdateZoom(float smothFactor)
        {
            if (_zoomTarget == _camera.Zoom)
                return;

            float diff = _zoomTarget - _camera.Zoom;
            diff = float.Abs(diff) < 1e-10 ? 0 : diff;
            _camera.Zoom += diff / smothFactor;

            if (diff == 0)
                _camera.Zoom = _zoomTarget;
        }

        public void Zoom(float value) => _zoomTarget = value;

        private void UpdatePositoin(float smothFactor)
        {
            if (_positionTarget == _camera.Position)
                return;

            Vector2 vectorDif = _positionTarget - _camera.Position;
            vectorDif.X = float.Abs(vectorDif.X) < 1e-10 ? 0 : vectorDif.X;
            vectorDif.Y = float.Abs(vectorDif.Y) < 1e-10 ? 0 : vectorDif.Y;
            _camera.Position += vectorDif / smothFactor;

            if (vectorDif.X == 0 && vectorDif.Y == 0)
                _camera.Position = _positionTarget;
        }

        public void ControllZoom(InputState inputState, float minZoom, float maxZoom, float amount = 200)
        {
            float zoom = 0f; int multiplier = 1;
            amount /= _elapsedMilliseconds;

            inputState.DoAction(ActionType.MouseWheelForward, () => zoom += amount);
            inputState.DoAction(ActionType.MouseWheelBackward, () => zoom -= amount);
            if (zoom == 0) return;

            _zoomTarget *= 1 + zoom * multiplier * 0.001f * _elapsedMilliseconds;
            _zoomTarget = MathHelper.Clamp(_zoomTarget, minZoom, maxZoom);
        }

        public void MoveByMouse(InputState inputState)
        {
            if (inputState.HasAction(ActionType.RightClickHold))
                _positionTarget += (_lastMousePosition - inputState.MousePosition) / _camera.Zoom;
            _lastMousePosition = inputState.MousePosition;
        }

        public void MoveToPosition(Vector2 position)
        {
            _positionTarget = position;
        }

        public void SetPosition(Vector2 position)
        {
            _positionTarget = position;
            _camera.Position = position;
        }

        public void EdgeScrolling(Vector2 mousePosition, Rectangle screenBound, float amount = 2)
        {
            Vector2 edge = screenBound.Size.ToVector2() * 0.10f;
            RectangleF bounds = new RectangleF(screenBound.Location.ToVector2() + edge / 2, screenBound.Size.ToVector2() - edge);
            if (bounds.Contains(mousePosition)) return;
            Vector2 dir = Vector2.Normalize(mousePosition - screenBound.Center.ToVector2());
            _positionTarget += dir * amount * _elapsedMilliseconds / _camera.Zoom;

            int x = (int)MathHelper.Clamp(mousePosition.X, screenBound.Left, screenBound.Right);
            int y = (int)MathHelper.Clamp(mousePosition.Y, screenBound.Top, screenBound.Bottom);
            Mouse.SetPosition(x, y);
        }
    }
}
