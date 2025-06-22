// Screen.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Input;
using GameEngine.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;

namespace GameEngine.Screens
{
    [Serializable]
    public abstract class Screen : IDisposable
    {
        [JsonIgnore] public Effect Effect { get; protected set; }
        [JsonIgnore] public readonly bool UpdateBelow;
        [JsonIgnore] public readonly bool DrawBelow;
        [JsonIgnore] public readonly bool BlurBelow;
        [JsonIgnore] protected readonly GameServiceContainer GameServices;
        [JsonIgnore] protected readonly ScreenManager ScreenManager;
        [JsonIgnore] protected readonly GraphicsDevice GraphicsDevice;
        [JsonIgnore] protected readonly UiFrame UiRoot;
        [JsonIgnore] private RenderTarget2D _renderTarget2D;

        protected Screen(GameServiceContainer gameServices, GraphicsDevice graphicsDevice, ScreenManager screenManager, bool updateBelow, bool drawBelow, bool blurBelow)
        {
            GameServices = gameServices;
            GraphicsDevice = graphicsDevice;
            ScreenManager = screenManager;
            UpdateBelow = updateBelow;
            DrawBelow = drawBelow;
            BlurBelow = blurBelow;

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

        public virtual void Update(GameTime gameTime, InputState inputState)
        {
            UiRoot.Update(inputState, GraphicsDevice.Viewport.Bounds);
        }

        public virtual void Draw(SpriteBatch spriteBatch) {; }

        public RenderTarget2D RenderTarget(SpriteBatch spriteBatch)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget2D);
            GraphicsDevice.Clear(Color.Transparent);
            Draw(spriteBatch);
            spriteBatch.Begin();
            UiRoot.Draw(spriteBatch);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            return _renderTarget2D;
        }

        public virtual void ApplyResolution(GameTime gameTime, float uiScale)
        {
            UiRoot.ApplyScale(GraphicsDevice.Viewport.Bounds, uiScale);

            _renderTarget2D?.Dispose();
            _renderTarget2D = new(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public virtual void Dispose()
        {
            _renderTarget2D.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}