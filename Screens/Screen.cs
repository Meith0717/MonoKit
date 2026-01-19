// Screen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Rendering;
using MonoKit.Input;
using MonoKit.Ui;

namespace MonoKit.Screens;

public abstract class Screen : IDisposable
{
    private readonly PostProcessingRunner _postProcessingRunner;

    protected readonly GameServiceContainer AppServices;
    public readonly bool DrawBelow;
    protected readonly GraphicsDevice GraphicsDevice;
    protected readonly ScreenManager ScreenManager;
    protected readonly UiFrame UiRoot;
    public readonly bool UpdateBelow;
    private RenderTarget2D _renderTarget;
    public IPostProcessingEffect PostProcessingEffect;

    protected Screen(GameServiceContainer appServices, bool updateBelow, bool drawBelow)
    {
        AppServices = appServices;
        GraphicsDevice = appServices.GetService<GraphicsDevice>();
        ScreenManager = appServices.GetService<ScreenManager>();
        UpdateBelow = updateBelow;
        DrawBelow = drawBelow;
        _postProcessingRunner = new PostProcessingRunner(GraphicsDevice);

        UiRoot = new UiFrame
        {
            Color = Color.Transparent,
            RelWidth = 1,
            RelHeight = 1,
            FillScale = FillScale.FillIn,
            Allign = Allign.Center,
        };
    }

    public virtual void Dispose()
    {
        _renderTarget.Dispose();
        _postProcessingRunner.Dispose();
        GC.SuppressFinalize(this);
    }

    public virtual void Initialize()
    {
        ;
    }

    public virtual void Update(double elapsedMilliseconds, InputHandler inputHandler, float uiScale)
    {
        UiRoot.Update(inputHandler, GraphicsDevice.Viewport.Bounds, uiScale);
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        ;
    }

    public virtual void ApplyResolution(double elapsedMilliseconds, float uiScale)
    {
        UiRoot.ApplyScale(GraphicsDevice.Viewport.Bounds, uiScale);
        _renderTarget?.Dispose();
        _renderTarget = new RenderTarget2D(
            GraphicsDevice,
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24Stencil8,
            4,
            RenderTargetUsage.PreserveContents
        );
        _postProcessingRunner.ApplyResolution(
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height
        );
    }

    public RenderTarget2D RenderTarget(SpriteBatch spriteBatch)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Transparent);
        Draw(spriteBatch);
        spriteBatch.Begin();
        UiRoot.Draw(spriteBatch);
        spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);

        if (PostProcessingEffect is not null)
            return PostProcessingEffect.Apply(spriteBatch, _postProcessingRunner, _renderTarget);

        return _renderTarget;
    }
}
