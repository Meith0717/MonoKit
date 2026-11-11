// MonoKit - IPostProcessedScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.App.Rendering;

namespace MonoKit.App.Screens;

/// <summary>An <see cref="IScreen" /> whose rendered output is passed through a post-processing effect before compositing.</summary>
public interface IPostProcessedScreen : IScreen
{
    /// <summary>Effect applied to this screen's output.</summary>
    IPostProcessingEffect PostProcessingEffect { get; }
}
