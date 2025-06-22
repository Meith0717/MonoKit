// MouseListener.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GameEngine.Input
{
    internal class MouseListener
    {
        private const double _clickHoldTeshholld = 75;
        private MouseState _currentState, _previousState;
        private double _leftCounter, _rightCounter, _midCounter;

        private bool LeftMouseButtonPressed => _currentState.LeftButton == ButtonState.Pressed;
        private bool RightMouseButtonPressed => _currentState.RightButton == ButtonState.Pressed;
        private bool MidMouseButtonPressed => _currentState.MiddleButton == ButtonState.Pressed;

        private bool LeftMouseButtonReleased => _currentState.LeftButton == ButtonState.Released;
        private bool RightMouseButtonReleased => _currentState.RightButton == ButtonState.Released;
        private bool MidMouseButtonReleased => _currentState.MiddleButton == ButtonState.Released;

        private bool LeftMouseButtonJustReleased => _currentState.LeftButton == ButtonState.Released && _previousState.LeftButton == ButtonState.Pressed;
        private bool RightMouseButtonJustReleased => _currentState.RightButton == ButtonState.Released && _previousState.RightButton == ButtonState.Pressed;
        private bool MidMouseButtonJustReleased => _currentState.MiddleButton == ButtonState.Released && _previousState.MiddleButton == ButtonState.Pressed;


        private readonly Dictionary<ActionType, ActionType> mKeyBindingsMouse = new()
            {
                { ActionType.MouseWheelBackward, ActionType.CameraZoomOut },
                { ActionType.MouseWheelForward, ActionType.CameraZoomIn },
            };

        public void Listen(GameTime gameTime, ref List<ActionType> actions, out Vector2 mousePosition)
        {
            _previousState = _currentState;
            _currentState = Mouse.GetState();
            mousePosition = _currentState.Position.ToVector2();

            // Track the time the Keys are Pressed
            _leftCounter += LeftMouseButtonPressed ? gameTime.ElapsedGameTime.TotalMilliseconds : 0;
            _rightCounter += RightMouseButtonPressed ? gameTime.ElapsedGameTime.TotalMilliseconds : 0;

            // Check if Mouse Key was Hold or Clicked
            if (_leftCounter > _clickHoldTeshholld && !LeftMouseButtonJustReleased)
                actions.Add(ActionType.LeftClickHold);
            if (_rightCounter > _clickHoldTeshholld && !RightMouseButtonJustReleased)
                actions.Add(ActionType.RightClickHold);
            if (_midCounter > _clickHoldTeshholld && !MidMouseButtonJustReleased)
                actions.Add(ActionType.MidClickHold);


            // Check for Mouse Key Pressed
            if (LeftMouseButtonJustReleased)
                actions.Add(ActionType.LeftReleased);
            if (RightMouseButtonJustReleased)
                actions.Add(ActionType.RightReleased);
            if (MidMouseButtonJustReleased)
                actions.Add(ActionType.MidReleased);

            // Check for Mouse Key Release
            if (LeftMouseButtonJustReleased)
                actions.Add(ActionType.LeftWasClicked);
            if (RightMouseButtonJustReleased)
                actions.Add(ActionType.RightWasClicked);
            if (MidMouseButtonJustReleased)
                actions.Add(ActionType.MidWasClicked);

            // Reset counters
            if (LeftMouseButtonReleased)
                _leftCounter = 0;
            if (RightMouseButtonReleased)
                _rightCounter = 0;
            if (MidMouseButtonReleased)
                _midCounter = 0;

            // Set Mouse Action to MouseWheel
            if (_currentState.ScrollWheelValue > _previousState.ScrollWheelValue)
                actions.Add(ActionType.MouseWheelForward);

            if (_currentState.ScrollWheelValue < _previousState.ScrollWheelValue)
                actions.Add(ActionType.MouseWheelBackward);

            foreach (ActionType key in mKeyBindingsMouse.Keys)
            {
                if (!actions.Contains(key)) continue;
                actions.Add(mKeyBindingsMouse[key]);
            }
        }
    }
}
