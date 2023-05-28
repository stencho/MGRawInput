using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static MGRawInputLib.InputStructs;

namespace MGRawInputLib {
    public static class Input {
        static Thread control_update_thread = new Thread(new ThreadStart(update));

        public static List<InputHandler> handlers = new List<InputHandler>();
        static Game parent;
        public static void initialize(Game parent) { 
            Input.parent = parent;

            Externs.RawInput.create_rawinput_message_loop(); 

            control_update_thread.Start(); 
        }
        public static void kill() { run_thread = false; }

        public static bool num_lock => keyboard_state.NumLock;
        public static bool caps_lock => keyboard_state.CapsLock;


        public static KeyboardState keyboard_state {get; set; }
        public static MouseState mouse_state { get; set; }
        public static MouseState mouse_state_previous { get; set; }

        public static GamePadState gamepad_one_state { get; private set; }
        public static GamePadState gamepad_two_state { get; private set; }
        public static GamePadState gamepad_three_state { get; private set; }
        public static GamePadState gamepad_four_state { get; private set; }

        public static int frame_rate => _frame_rate;
        static int _frame_rate;

        static double _fps_timer = 0;
        static long _frame_count = 0;

        public static int fps_update_frequency_ms { get; set; } = 500;
        public static int poll_hz { get; private set; } = 1000;

        static double thread_ms => (1000.0 / poll_hz);

        static DateTime start_dt = DateTime.Now;
        static DateTime current_dt = DateTime.Now;
        static TimeSpan ts;

        static volatile bool run_thread = true;

        public static bool lock_mouse = false;
        static bool _was_locked = false;

        public enum input_method { MonoGame, RawInput, Both }
        static input_method _input_method = input_method.MonoGame;
        public static input_method poll_method => _input_method;
        public static void change_polling_method(input_method method) {
            if (_input_method != input_method.RawInput && method == input_method.RawInput) {
                Externs.RawInput.enable = true;
            } else if (_input_method != input_method.Both && method == input_method.Both) {
                Externs.RawInput.enable = true;
            } else if (_input_method != input_method.MonoGame && method == input_method.MonoGame) {
                Externs.RawInput.enable = false;
            }
            _input_method = method;
        }

        public static Point mouse_delta = Point.Zero;
        static bool was_active = false;

        public static Vector2 mouse_delta_rawinput;

        static Point pre_lock_mouse_pos = Point.Zero;

        public static Point cursor_pos;
        public static Point cursor_pos_actual;


        public static string RAWINPUT_DEBUG_STRING = "";


        public static void hide_mouse() { parent.IsMouseVisible = false; }
        public static void show_mouse() { parent.IsMouseVisible = true; }

        public static bool moving_window { 
            get { 
                return _mv_wnd;  
            } set {
                if (_mv_wnd != value && value)
                    relative_mouse = Externs.get_cursor_pos() - parent.Window.ClientBounds.Location;

                _mv_wnd = value;
            }
        } static bool _mv_wnd = false;

        public static Point relative_mouse;

        static void update() {
            while (run_thread) {
                start_dt = DateTime.Now;

                cursor_pos = Externs.get_cursor_pos_relative_to_window();
                cursor_pos_actual = Externs.get_cursor_pos();

                if (_input_method == input_method.Both || _input_method == input_method.MonoGame) {
                    keyboard_state = Keyboard.GetState();
                    mouse_state_previous = mouse_state;
                    mouse_state = Mouse.GetState();

                    mouse_delta = (mouse_state.Position - mouse_state_previous.Position);

                    foreach(InputHandler handler in handlers) handler.mouse_delta_accumulated += mouse_delta;

                    gamepad_one_state = GamePad.GetState(PlayerIndex.One);
                    gamepad_two_state = GamePad.GetState(PlayerIndex.Two);
                    gamepad_three_state = GamePad.GetState(PlayerIndex.Three);
                    gamepad_four_state = GamePad.GetState(PlayerIndex.Four);
                } 
                
                if (_input_method == input_method.Both || _input_method == input_method.RawInput) {
                    //rawinput polling here

                    
                }

                //mouse lock 
                if (parent != null && lock_mouse && !_was_locked) {
                    parent.IsMouseVisible = false;
                    pre_lock_mouse_pos = mouse_state.Position;
                }
                if (lock_mouse && parent.IsActive) {
                    reset_mouse(parent.Window.ClientBounds.Size);
                }
                if ((parent != null && !parent.IsActive && was_active && lock_mouse) || (!lock_mouse && _was_locked)) {
                    parent.IsMouseVisible = true;
                    Mouse.SetPosition(pre_lock_mouse_pos.X, pre_lock_mouse_pos.Y);
                }

                if (moving_window) {
                    var rec = Externs.get_window_rect();
                    var cp = Externs.get_cursor_pos();

                    if (mouse_delta.X != 0 || mouse_delta.Y != 0) {
                        Externs.MoveWindow(Externs.actual_window_handle,
                            cp.X - relative_mouse.X,
                            cp.Y - relative_mouse.Y,
                            rec.Width, rec.Height, false);                        
                    }
                }
                

                was_active = parent.IsActive;
                _was_locked = lock_mouse;
                //FPS stuff here
                _fps_timer += ts.TotalMilliseconds;
                _frame_count++;

                if (_fps_timer >= fps_update_frequency_ms) {
                    _frame_rate = (int)(_frame_count * (1000.0 / fps_update_frequency_ms));
                    _frame_count = 0;
                    _fps_timer -= fps_update_frequency_ms;
                }

                while (run_thread) {
                    current_dt = DateTime.Now;
                    ts = (current_dt - start_dt);
                    if (ts.TotalMilliseconds >= thread_ms) break;
                }
            }
        }
        public static bool is_pressed(Keys key) {
            if (_input_method == input_method.MonoGame) return keyboard_state.IsKeyDown(key);
            else if (_input_method == input_method.RawInput) return false;
            else return keyboard_state.IsKeyDown(key) || false;
        }
        
        public static bool is_pressed(MouseButtons mouse_button) {
            if (_input_method == input_method.MonoGame) {
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
                    default: return false;
                }
            } 
            else if (_input_method == input_method.RawInput) return false;
            else return false;
        }

        static void reset_mouse(Point resolution) {
            Mouse.SetPosition(resolution.X/2, resolution.Y/2);
        }

    }
}

