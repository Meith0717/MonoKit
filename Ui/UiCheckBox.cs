// UiCheckBox.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace GameEngine.Ui
{
    public class UiCheckBox : UiButton
    {
        public bool State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                Texture = value ? "ui_toggle_true" : "ui_toggle_false";
            }
        }
        private bool _state;

        public UiCheckBox()
            : base("")
        {
            Texture = State ? "ui_toggle_true" : "ui_toggle_false";
            OnClickAction = () =>
            {
                State = !State;
                Texture = State ? "ui_toggle_true" : "ui_toggle_false";
            };
        }
    }
}
