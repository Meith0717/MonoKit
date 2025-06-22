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
                Texture = value ? "toggle_on" : "toggle_off";
            }
        }
        private bool _state;

        public UiCheckBox()
            : base("")
        {
            Texture = State ? "toggle_on" : "toggle_off";
            OnClickAction = () =>
            {
                State = !State;
                Texture = State ? "toggle_on" : "toggle_off";
            };
        }
    }
}
