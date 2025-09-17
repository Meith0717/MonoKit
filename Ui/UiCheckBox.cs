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
                AttachTexture(State ? "ui_toggle_true" : "ui_toggle_false", 1);
            }
        }
        private bool _state;

        public UiCheckBox()
            : base(null, null, "")
        {
            AttachTexture(State ? "ui_toggle_true" : "ui_toggle_false", 1);
            OnClickAction = () => State = !State;
        }
    }
}
