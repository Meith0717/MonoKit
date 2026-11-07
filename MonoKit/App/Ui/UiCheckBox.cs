// UiCheckBox.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework.Graphics;

namespace MonoKit.App.Ui;

public sealed class UiCheckBox : UiButton.Sprite
{
    private readonly Texture2D _falseButton;
    private readonly Texture2D _trueButton;
    private bool _state;

    public UiCheckBox(Texture2D trueButton, Texture2D falseButton)
        : base(trueButton)
    {
        OnClickAction += () => State = !State;
        _trueButton = trueButton;
        _falseButton = falseButton;
    }

    public bool State
    {
        get => _state;
        set => Texture2D = _state == value ? _trueButton : _falseButton;
    }
}
