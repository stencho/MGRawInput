using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib.InputTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static MGRawInputLib.InputStructs;

namespace MGRawInputLib {
    //This entire class exists to "decouple" the input handling from the polling thread
    //Basically, if the polling thread handles the previous/current state, just_pressed
    //and just_released won't work correctly, as, to put it in some kinda way, the rising
    //edge of the pressed state is happening between frames
    //If the main thread is used for polling, then any threads used for updating will have
    //the same issue if they're running slower than it, or the opposite problem, too much
    //just_pressed, if they're running faster

    //The program will crash if say Keyboard.GetState() gets called in two different threads
    //around the same time, so we have one high rate thread to poll the inputs, then other
    //threads copy from it and handle their own previous state

    //same thing kind of applies for mouse_delta

    public class InputManager {
        public Vector2 mouse_delta_integer { get; private set; }
        public Vector2 mouse_position { get; private set; }
        public int scroll_value { get; private set; }
        int scroll_value_previous;

        KeyboardState keyboard_state; KeyboardState keyboard_state_previous;

        MouseState mouse_state; MouseState mouse_state_previous;

        GamePadState gamepad_one_state; GamePadState gamepad_one_state_previous;
        GamePadState gamepad_two_state; GamePadState gamepad_two_state_previous;
        GamePadState gamepad_three_state; GamePadState gamepad_three_state_previous;
        GamePadState gamepad_four_state; GamePadState gamepad_four_state_previous;

        public void update() {
            keyboard_state_previous = keyboard_state;
            keyboard_state = InputPolling.keyboard_state;
            
            mouse_state_previous = mouse_state;
            mouse_state = InputPolling.mouse_state;

            gamepad_one_state_previous = gamepad_one_state;
            gamepad_one_state = InputPolling.gamepad_one_state;

            gamepad_two_state_previous = gamepad_two_state;
            gamepad_two_state = InputPolling.gamepad_two_state;

            gamepad_three_state_previous = gamepad_three_state;
            gamepad_three_state = InputPolling.gamepad_three_state;

            gamepad_four_state_previous = gamepad_four_state;
            gamepad_four_state = InputPolling.gamepad_four_state;
            
            mouse_position = mouse_state.Position.ToVector2();
            mouse_delta_integer = (mouse_state.Position - mouse_state_previous.Position).ToVector2();
            scroll_value_previous = scroll_value;
            scroll_value = mouse_state.ScrollWheelValue;
            
        }

        public bool is_pressed(Keys key) {
            return keyboard_state.IsKeyDown(key);            
        }
        public bool was_pressed(Keys key) {
            return keyboard_state_previous.IsKeyDown(key);
        }
        public bool is_pressed(MouseButtons mouse_button) {
            switch (mouse_button) {
                case MouseButtons.Left:
                    return mouse_state.LeftButton == ButtonState.Pressed;
                case MouseButtons.Right:
                    return mouse_state.RightButton == ButtonState.Pressed;
                case MouseButtons.Middle:
                    return mouse_state.MiddleButton == ButtonState.Pressed;
                case MouseButtons.X1:
                    return mouse_state.XButton1 == ButtonState.Pressed;
                case MouseButtons.X2:
                    return mouse_state.XButton2 == ButtonState.Pressed;
                case MouseButtons.ScrollUp:
                    return (scroll_value - scroll_value_previous) > 0;
                case MouseButtons.ScrollDown:
                    return (scroll_value - scroll_value_previous) < 0;
                default: return false;
            }
        }
        public bool was_pressed(MouseButtons mouse_button) {
            switch (mouse_button) {
                case MouseButtons.Left:
                    return mouse_state_previous.LeftButton == ButtonState.Pressed;
                case MouseButtons.Right:
                    return mouse_state_previous.RightButton == ButtonState.Pressed;
                case MouseButtons.Middle:
                    return mouse_state_previous.MiddleButton == ButtonState.Pressed;
                case MouseButtons.X1:
                    return mouse_state_previous.XButton1 == ButtonState.Pressed;
                case MouseButtons.X2:
                    return mouse_state_previous.XButton2 == ButtonState.Pressed;
                case MouseButtons.ScrollUp:
                    return mouse_state_previous.ScrollWheelValue > 0;
                case MouseButtons.ScrollDown:
                    return mouse_state_previous.ScrollWheelValue < 0;
                default: return false;
            }
        }

        public bool just_pressed(Keys key) => is_pressed(key) && !was_pressed(key);        
        public bool just_released(Keys key) => !is_pressed(key) && was_pressed(key);

        public bool just_pressed(MouseButtons mouse_button) => is_pressed(mouse_button) && !was_pressed(mouse_button);
        public bool just_released(MouseButtons mouse_button) => !is_pressed(mouse_button) && was_pressed(mouse_button);
    }
}
