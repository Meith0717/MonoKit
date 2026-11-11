// MonoKit - UiHBox.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;

namespace MonoKit.App.Ui;

/// <summary>
///     Horizontal row of equal cells (minus <see cref="Gap" />). Children keep any explicitly
///     set RelWidth/RelHeight captured at <see cref="Add" />; defaults fill the cell.
/// </summary>
public class UiHBox : UiContainer
{
    private const float DefaultSlot = 0.1f;
    private readonly Dictionary<UiElement, float> _baseHeight = new();
    private readonly Dictionary<UiElement, float> _baseWidth = new();

    /// <summary>Fraction of each cell reserved as gap (0 = no gap).</summary>
    public float Gap = 0;

    /// <summary>Adds a child and captures its explicitly set relative size.</summary>
    public override void Add(UiElement child)
    {
        _baseWidth[child] = child.RelWidth;
        _baseHeight[child] = child.RelHeight;
        base.Add(child);
    }

    /// <summary>Removes all children and their captured base sizes.</summary>
    public override void Clear()
    {
        _baseWidth.Clear();
        _baseHeight.Clear();
        base.Clear();
    }

    /// <summary>Places children in equal horizontal cells minus the gap.</summary>
    protected override void LayoutChildren()
    {
        var count = Children.Count;
        if (count == 0)
            return;

        var cell = 1f / count;
        var i = 0;
        foreach (var child in Children)
        {
            child.RelX = i * cell;
            child.RelY = 0;
            child.RelWidth =
                _baseWidth.TryGetValue(child, out var w) && w != DefaultSlot ? w : cell * (1 - Gap);
            child.RelHeight = _baseHeight.TryGetValue(child, out var h) && h != DefaultSlot ? h : 1;
            child.Align = Align.None;
            child.FillScale = FillScale.None;
            i++;
        }
    }
}
