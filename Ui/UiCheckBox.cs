// UiCheckBox.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace GameEngine.Ui
{
    public sealed class UiCheckBox : UiButton.Sprite
    {
        private bool _state;
        private readonly string _trueButton;
        private readonly string _falseButton;

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
}
