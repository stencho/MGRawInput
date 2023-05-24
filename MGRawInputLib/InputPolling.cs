using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MGRawInputLib {
    public static class InputPolling {
        static Thread control_update_thread = new Thread(new ThreadStart(update));

        public static void initialize() { control_update_thread.Start(); }
        public static void kill() { run_thread = false; }


        public static KeyboardState keyboard_state {get; private set; }
        public static MouseState mouse_state { get; private set; }

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
        public static void update() {
            while (run_thread) {
                start_dt = DateTime.Now;

                keyboard_state = Keyboard.GetState();
                mouse_state = Mouse.GetState();

                gamepad_one_state = GamePad.GetState(PlayerIndex.One);
                gamepad_two_state = GamePad.GetState(PlayerIndex.Two);
                gamepad_three_state = GamePad.GetState(PlayerIndex.Three);
                gamepad_four_state = GamePad.GetState(PlayerIndex.Four);

                while (run_thread) {
                    current_dt = DateTime.Now;
                    ts = (current_dt - start_dt);
                    if (ts.TotalMilliseconds >= thread_ms) break;
                }

                //FPS stuff here
                _fps_timer += ts.TotalMilliseconds;
                _frame_count++;

                if (_fps_timer >= fps_update_frequency_ms) {
                    _frame_rate = (int)(_frame_count * (1000.0 / fps_update_frequency_ms));
                    _frame_count = 0;
                    _fps_timer -= fps_update_frequency_ms;
                }
            }
        }

        static void reset_mouse() {

        }

    }
}
