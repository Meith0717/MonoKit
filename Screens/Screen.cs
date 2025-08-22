// Screen.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using GameEngine.Rendering;
using GameEngine.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngine.Screens
{
    public abstract class Screen : IDisposable
    {
        public readonly bool UpdateBelow;
        public readonly bool DrawBelow;

        protected readonly GameServiceContainer ApplicationServices;
        protected readonly ScreenManager ScreenManager;
        protected readonly GraphicsDevice GraphicsDevice;
        protected readonly UiFrame UiRoot;

        private readonly PostProcessing _postProcessing;
        private RenderTarget2D _renderTarget;


        protected Screen(GameServiceContainer applicationServices, bool updateBelow, bool drawBelow)
        {
            ApplicationServices = applicationServices;
            GraphicsDevice = applicationServices.GetService<GraphicsDevice>();
            ScreenManager = applicationServices.GetService<ScreenManager>();
            UpdateBelow = updateBelow;
            DrawBelow = drawBelow;
            _postProcessing = new(GraphicsDevice);

            UiRoot = new()
            {
                Alpha = 0,
                RelWidth = 1,
                RelHeight = 1,
                FillScale = FillScale.Fit,
                Anchor = Anchor.Center,
            };
        }

        public virtual void Initialize() {; }

        public virtual void Update(GameTime gameTime, InputState inputState, float uiScale)
        {
            UiRoot.Update(inputState, GraphicsDevice.Viewport.Bounds, uiScale);
        }

        public virtual void Draw(SpriteBatch spriteBatch) {; }

        public virtual void ApplyResolution(GameTime gameTime, float uiScale)
        {
            UiRoot.ApplyScale(GraphicsDevice.Viewport.Bounds, uiScale);
            _renderTarget?.Dispose();
            _renderTarget = new(GraphicsDevice,
                                  GraphicsDevice.Viewport.Width,
                                  GraphicsDevice.Viewport.Height,
                                  false,
                                  SurfaceFormat.Color,
                                  DepthFormat.Depth24Stencil8,
                                  4,
                                  RenderTargetUsage.PreserveContents);
            _postProcessing.ApplyResolution(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public virtual void Dispose()
        {
            _renderTarget.Dispose();
            GC.SuppressFinalize(this);
        }

        public PostProcessingDelegate PostProcessing { get; set; }

        public RenderTarget2D RenderTarget(SpriteBatch spriteBatch)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            Draw(spriteBatch);
            spriteBatch.Begin();
            UiRoot.Draw(spriteBatch);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            if (PostProcessing is not null)
                return PostProcessing.Invoke(spriteBatch, _postProcessing, _renderTarget);

            return _renderTarget;
        }
    }
}