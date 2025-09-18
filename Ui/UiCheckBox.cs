// UiCheckBox.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace GameEngine.Ui
{
    public sealed class UiCheckBox : UiButton
    {
        private bool _state;

        public UiCheckBox() : base("ui_toggle_true")
        {
            OnClickAction = () => State = !State;
        }

        public bool State
        {
            get => _state;
            set => Texture = (_state = value) ? "ui_toggle_true" : "ui_toggle_false";
        }
    }
}
