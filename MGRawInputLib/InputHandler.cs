﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class InputHandler {
        public Point mouse_delta { get; private set; }

        KeyboardState key_state;
        KeyboardState key_state_previous;

        public RawInputKeyboardState rawinput_key_state;
        public RawInputKeyboardState rawinput_key_state_previous;

        MouseState mouse_state;
        MouseState mouse_state_previous;

        public RawInputMouseState rawinput_mouse_state;
        public RawInputMouseState rawinput_mouse_state_previous;

        Point _mouse_delta_acc = Point.Zero;
        Point mouse_delta_accumulated { 
            get {
                var ret = _mouse_delta_acc;
                _mouse_delta_acc = Point.Zero;
                return ret;
            } 
            set {
                _mouse_delta_acc = value;
            } 
        }

        public Vector2 mouse_position { get; private set; }

        int _scroll_delta_acc;
        int scroll_delta_accumulated {
            get {
                var ret = _scroll_delta_acc;
                _scroll_delta_acc = 0;
                return ret;
            } 
            set {
                _scroll_delta_acc = value;
            }
        }
        internal void accumulate_scroll_delta(int a) {
            _scroll_delta_acc += a;
        }
        internal void accumulate_mouse_delta(Point p) {
            _mouse_delta_acc += p;
        }


        public InputHandler() {
            Input.handlers.Add(this);

            rawinput_key_state_previous = RawInputKeyboard.GetState();
            rawinput_key_state = RawInputKeyboard.GetState();
        }

        ~InputHandler() { Input.handlers.Remove(this); }

        public string ri_info() {
            return $"[keyboard] -> {rawinput_key_state.list_keys()}\n[mouse] -> {rawinput_mouse_state.info()}";
        }

        public void update() {
            if (Input.poll_method == Input.input_method.MonoGame) {
                key_state_previous = key_state;
                key_state = Input.keyboard_state;

                mouse_state_previous = mouse_state;
                mouse_state = Input.mouse_state;

            } else {
                rawinput_key_state_previous = rawinput_key_state;
                rawinput_key_state = Input.ri_keyboard_state;      
                
                rawinput_mouse_state_previous = rawinput_mouse_state;
                rawinput_mouse_state = Input.ri_mouse_state;

            }

            mouse_position = Input.cursor_pos.ToVector2();

            mouse_delta = mouse_delta_accumulated;

            rawinput_mouse_state.Delta = mouse_delta;
            rawinput_mouse_state.ScrollDelta = scroll_delta_accumulated;
        }

        public bool is_pressed(Keys key) {
            if (Input.poll_method == Input.input_method.MonoGame) {
                return key_state.IsKeyDown(key);
            } else {
                return rawinput_key_state.IsKeyDown(key);
            }
        }
        public bool was_pressed(Keys key) {
            if (Input.poll_method == Input.input_method.MonoGame) {
                return key_state_previous.IsKeyDown(key);
            } else {
                return rawinput_key_state_previous.IsKeyDown(key);
            }
        }

        public bool is_pressed(MouseButtons mouse_button) {
            if (Input.poll_method == Input.input_method.MonoGame) {
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
                        return mouse_state.ScrollWheelValue > mouse_state_previous.ScrollWheelValue;
                    case MouseButtons.ScrollDown:
                        return mouse_state.ScrollWheelValue < mouse_state_previous.ScrollWheelValue;
                }
            } else {
                switch (mouse_button) {
                    case MouseButtons.Left:
                        return rawinput_mouse_state.LeftButton;                        
                    case MouseButtons.Right:
                        return rawinput_mouse_state.RightButton;
                    case MouseButtons.Middle:
                        return rawinput_mouse_state.MiddleButton;
                    case MouseButtons.X1:
                        return rawinput_mouse_state.XButton1;
                    case MouseButtons.X2:
                        return rawinput_mouse_state.XButton2;
                    case MouseButtons.ScrollUp:
                        return rawinput_mouse_state.ScrollUp;
                    case MouseButtons.ScrollDown:
                        return rawinput_mouse_state.ScrollDown;
                }
            }
            return false;
        }

        public bool was_pressed(MouseButtons mouse_button) {
            if (Input.poll_method == Input.input_method.MonoGame) {
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
                        return false;
                    case MouseButtons.ScrollDown:
                        return false;
                }
            } else {
                switch (mouse_button) {
                    case MouseButtons.Left:
                        return rawinput_mouse_state_previous.LeftButton;
                    case MouseButtons.Right:
                        return rawinput_mouse_state_previous.RightButton;
                    case MouseButtons.Middle:
                        return rawinput_mouse_state_previous.MiddleButton;
                    case MouseButtons.X1:
                        return rawinput_mouse_state_previous.XButton1;
                    case MouseButtons.X2:
                        return rawinput_mouse_state_previous.XButton2;
                    case MouseButtons.ScrollUp:
                        return rawinput_mouse_state_previous.ScrollUp;
                    case MouseButtons.ScrollDown:
                        return rawinput_mouse_state_previous.ScrollDown;
                }
            }
            return false;
        }

        public bool just_pressed(Keys key) => is_pressed(key) && !was_pressed(key);        
        public bool just_released(Keys key) => !is_pressed(key) && was_pressed(key);

        public bool just_pressed(MouseButtons mouse_button) => is_pressed(mouse_button) && !was_pressed(mouse_button);
        public bool just_released(MouseButtons mouse_button) => !is_pressed(mouse_button) && was_pressed(mouse_button);
    }
}

