// ZoomBehaviour.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace GameEngine.Camera
{
    public class ZoomByMouse
    {
        public class In2D(Range<float> zoomRange, float smooth) : ICamera2dBehaviour
        {
            private readonly float _smooth = smooth;
            private readonly Range<float> _zoomRange = zoomRange;
            private float _zoomTarget;

            public void Initialize(Camera2D owner)
            {
                _zoomTarget = owner.Zoom;
            }

            public void Update(Camera2D owner, InputState inputState, double elapsedGameTime)
            {
                float zoomDelta = 0f;

                inputState.DoAction(ActionType.MouseWheelForward, () => zoomDelta += 1f);
                inputState.DoAction(ActionType.MouseWheelBackward, () => zoomDelta -= 1f);

                if (zoomDelta != 0)
                {
                    _zoomTarget *= 1 + zoomDelta * 0.2f;
                    _zoomTarget = MathHelper.Clamp(_zoomTarget, _zoomRange.Min, _zoomRange.Max);
                }

                float lerpSpeed = (float)(elapsedGameTime * _smooth);
                owner.Zoom = MathHelper.Lerp(owner.Zoom, _zoomTarget, lerpSpeed);
            }
        }

        public class In3D(float smooth) : ICamera3dBehaviour
        {
            private readonly float _smooth = smooth;
            private float _zoomTarget;

            public void Initialize(Camera3D owner)
            {
                _zoomTarget = owner.Fov;
            }

            public void Update(Camera3D owner, InputState inputState, double elapsedGameTime)
            {
                float zoomDelta = 0f;

                inputState.DoAction(ActionType.MouseWheelForward, () => zoomDelta += .5f);
                inputState.DoAction(ActionType.MouseWheelBackward, () => zoomDelta -= .5f);

                if (zoomDelta != 0)
                {
                    _zoomTarget *= 1 + zoomDelta * 0.2f; // 10% zoom per tick
                    _zoomTarget = MathHelper.Clamp(_zoomTarget, 0, float.Pi);
                }

                float lerpSpeed = (float)(elapsedGameTime * _smooth);
                owner.Fov = MathHelper.Lerp(owner.Fov, _zoomTarget, lerpSpeed);
            }
        }
    }
}
