// UiCheckBox.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace MonoKit.Ui;

public sealed class UiCheckBox : UiButton.Sprite
{
    private readonly string _falseButton;
    private readonly string _trueButton;
    private bool _state;

    public UiCheckBox(string trueButton, string falseButton) : base(trueButton)
    {
        OnClickAction += () => State = !State;
        _trueButton = trueButton;
        _falseButton = falseButton;
    }

    public bool State
    {
        get => _state;
        set => Texture = (_state = value) ? _trueButton : _falseButton;
    }
}