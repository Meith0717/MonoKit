// IPostProcessedScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.App.Graphics.Rendering;

namespace MonoKit.App.Screens;

/// <summary>An <see cref="IScreen"/> whose rendered output is passed through a post-processing effect before compositing.</summary>
public interface IPostProcessedScreen : IScreen
{
    /// <summary>Effect applied to this screen's output.</summary>
    IPostProcessingEffect PostProcessingEffect { get; }
}
