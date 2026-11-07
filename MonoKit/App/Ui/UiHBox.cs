// AppHost - UiHBox.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace MonoKit.App.Ui;

/// <summary>Horizontal box: children share the width equally, full height.</summary>
public class UiHBox : UiContainer
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
            child.RelX = i * cell;
            child.RelY = 0;
            child.RelWidth = cell * (1 - Gap);
            child.RelHeight = 1;
            child.Align = Align.None;
            child.FillScale = FillScale.None;
        }
    }
}
