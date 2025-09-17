// UiCheckBox.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace GameEngine.Ui
{
    public class UiCheckBox : UiButton
    {
        public bool State
        {
            get => _state;
            set => Texture = (_state = value) ? "ui_toggle_true" : "ui_toggle_false";
        }
        private bool _state;

        public UiCheckBox() : base("ui_toggle_true")
        {
            Texture = State ? "ui_toggle_true" : "ui_toggle_false";
            OnClickAction = () => State = !State;
        }
    }
}
