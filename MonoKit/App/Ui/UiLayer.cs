// UiLayer.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace MonoKit.App.Ui;

/// <summary>
/// Plain grouping container: unlike VBox/HBox it does NOT override children's
/// relative slots — children keep their own RelX/RelY/RelWidth/RelHeight.
/// Set <see cref="UiContainer.ClipChildren" /> to clip rendering to the layer's bounds.
/// </summary>
public class UiLayer : UiContainer
{
    protected override void LayoutChildren() { }
}
