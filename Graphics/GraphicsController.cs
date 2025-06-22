// GraphicsController.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace Engine.Graphics
{
    public enum WindowMode { FullScreen, Borderless, Windowed }

    public class GraphicsController
    {
        private int _oldWidth;
        private int _oldHeight;

        private readonly Game _game;
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly GameWindow _window;
        private bool _resolutionWasResized;

        public GraphicsController(Game game, GameWindow window, GraphicsDeviceManager graphicsManager)
        {
            _game = game;
            _graphicsDeviceManager = graphicsManager;
            _graphicsDeviceManager.ApplyChanges();
            window.ClientSizeChanged += delegate { _resolutionWasResized = true; };
            _window = window;

            _oldWidth = 800;
            _oldHeight = 400;
        }

        private Size MonitorSize
            => new(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

        public bool ResolutionWasResized
            => (_resolutionWasResized, _resolutionWasResized = false).Item1;

        public void ApplyRefreshRate(int value)
        {
            _game.IsFixedTimeStep = value > 0;
            if (!_game.IsFixedTimeStep) return;
            _game.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / value);
            _graphicsDeviceManager.ApplyChanges();
        }

        public void ApplyVSync(bool value)
        {
            _graphicsDeviceManager.SynchronizeWithVerticalRetrace = value;
            _graphicsDeviceManager.ApplyChanges();
        }

        public void ApplyMode(WindowMode mode)
        {
            switch (mode)
            {
                case WindowMode.Windowed:
                    DoWindowMode();
                    break;
                case WindowMode.FullScreen:
                    DoFullScreen();
                    break;
                case WindowMode.Borderless:
                    DoBorderlessWindowed();
                    break;
            }
            _graphicsDeviceManager.ApplyChanges();
        }

        private void DoFullScreen()
        {
            if (!_graphicsDeviceManager.IsFullScreen)
            {
                _oldWidth = _window.ClientBounds.Width;
                _oldHeight = _window.ClientBounds.Height;
            }

            _graphicsDeviceManager.PreferredBackBufferHeight = MonitorSize.Height;
            _graphicsDeviceManager.PreferredBackBufferWidth = MonitorSize.Width;

            _graphicsDeviceManager.HardwareModeSwitch = true;
            _graphicsDeviceManager.IsFullScreen = true;
        }

        private void DoBorderlessWindowed()
        {
            if (!_graphicsDeviceManager.IsFullScreen)
            {
                _oldWidth = _window.ClientBounds.Width;
                _oldHeight = _window.ClientBounds.Height;
            }

            _graphicsDeviceManager.PreferredBackBufferHeight = MonitorSize.Height;
            _graphicsDeviceManager.PreferredBackBufferWidth = MonitorSize.Width;

            _graphicsDeviceManager.HardwareModeSwitch = false;
            _graphicsDeviceManager.IsFullScreen = true;
        }

        private void DoWindowMode()
        {
            _graphicsDeviceManager.PreferredBackBufferHeight = _oldHeight;
            _graphicsDeviceManager.PreferredBackBufferWidth = _oldWidth;

            _graphicsDeviceManager.IsFullScreen = false;
            _window.AllowUserResizing = true;
        }
    }
}
