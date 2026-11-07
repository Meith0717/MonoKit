// AppHost - UiCheckBox.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

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
        set
        {
            _state = value;
            Texture2D = value ? _trueButton : _falseButton;
        }
    }
}
