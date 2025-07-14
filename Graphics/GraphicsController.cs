// GraphicsController.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using MathNet.Numerics.Distributions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Linq;

namespace GameEngine.Graphics
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
            _graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            _graphicsDeviceManager.ApplyChanges();
            _window = window;
            _window.ClientSizeChanged += delegate 
            {
                _oldWidth = _window.ClientBounds.Width;
                _oldHeight = _window.ClientBounds.Height;
                _resolutionWasResized = true; 
            };

            _oldWidth = 800;
            _oldHeight = 400;
        }

        public void ApplyRefreshRate(int value, bool vSync)
        {

            if (value <= 0) throw new InvalidCastException();

            _graphicsDeviceManager.SynchronizeWithVerticalRetrace = vSync;
            _game.IsFixedTimeStep = !vSync;

            if (!vSync)
                _game.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / value);

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
            _resolutionWasResized = true;
        }

        public bool ResolutionWasResized => (_resolutionWasResized, _resolutionWasResized = false).Item1;

        private static Size MonitorSize => new(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

        private static int QualityLevels => 0;

        private void DoFullScreen()
        {
            _graphicsDeviceManager.PreferredBackBufferHeight = MonitorSize.Height;
            _graphicsDeviceManager.PreferredBackBufferWidth = MonitorSize.Width;

            _graphicsDeviceManager.HardwareModeSwitch = true;
            _graphicsDeviceManager.IsFullScreen = true;
        }

        private void DoBorderlessWindowed()
        {
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
