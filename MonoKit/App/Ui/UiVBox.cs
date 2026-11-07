// AppHost - UiVBox.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace MonoKit.App.Ui;

/// <summary>Vertical box: children share the height equally, full width.</summary>
public class UiVBox : UiContainer
{
    /// <summary>Fraction of each cell reserved as gap (0 = no gap).</summary>
    public float Gap = 0;

    protected override void LayoutChildren()
    {
        var count = Children.Count;
        if (count == 0)
            return;

        var cell = 1f / count;
        for (var i = 0; i < count; i++)
        {
            var child = Children[i];
            child.RelX = 0;
            child.RelY = i * cell;
            child.RelWidth = 1;
            child.RelHeight = cell * (1 - Gap);
            child.Align = Align.None;
            child.FillScale = FillScale.None;
        }
    }
}
