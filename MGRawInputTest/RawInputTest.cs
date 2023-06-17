using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MGRawInputLib;
using MGRawInputTest.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGRawInputTest {
    static class Extensions {
        public static Vector2 X_only(this Vector2 v) => new Vector2(v.X, 0);
        public static Vector2 Y_only(this Vector2 v) => new Vector2(0, v.Y);
    }

    public class RawInputTest : Game {
        GraphicsDeviceManager graphics;
        public static double target_fps = 60;
        
        FPSCounter fps;

        public static Vector2 resolution = new Vector2(850, 600);

        Texture2D tx_key_arrow;

        Texture2D tx_mouse_base, 
            tx_mouse_left,
            tx_mouse_right,
            tx_mouse_middle,
            tx_mouse_scroll_up, 
            tx_mouse_scroll_down,
            tx_mouse_xbutton1,
            tx_mouse_xbutton2;

        UIElementManager ui;

        public RawInputTest() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            this.IsFixedTimeStep = true;
            this.InactiveSleepTime = System.TimeSpan.Zero;
            this.TargetElapsedTime = System.TimeSpan.FromMilliseconds(1000.0 / target_fps);
            Window.IsBorderless = true;
            Window.Title = "MGRawInputTest";

            graphics.SynchronizeWithVerticalRetrace = true;
            
            graphics.PreferredBackBufferWidth = (int)resolution.X;
            graphics.PreferredBackBufferHeight = (int)resolution.Y;

            graphics.ApplyChanges();
        }

        protected override void Initialize() {
            Input.initialize(this);

            ui = new UIElementManager();
            
            fps = new FPSCounter();
            this.Disposed += RawInputTest_Disposed;
            base.Initialize();
        }

        private void RawInputTest_Disposed(object sender, System.EventArgs e) {
            Input.kill();
        }

        protected override void LoadContent() {
            Drawing.load(GraphicsDevice, graphics, Content);

            tx_key_arrow = Content.Load<Texture2D>("key_arrow");
            
            tx_mouse_base = Content.Load<Texture2D>("mouse/mouse_base");
            tx_mouse_left = Content.Load<Texture2D>("mouse/left_button");
            tx_mouse_right = Content.Load<Texture2D>("mouse/right_button");
            tx_mouse_middle = Content.Load<Texture2D>("mouse/middle_button");
            tx_mouse_scroll_up = Content.Load<Texture2D>("mouse/scroll_up");
            tx_mouse_scroll_down = Content.Load<Texture2D>("mouse/scroll_down");
            tx_mouse_xbutton1 = Content.Load<Texture2D>("mouse/xbutton1");
            tx_mouse_xbutton2 = Content.Load<Texture2D>("mouse/xbutton2");

            build_UI();

            SDF.load(Content);
        }

        protected void build_UI() {
            ui.add_element("mouse_drift", new MouseDeltaDriftTest(bottom_section_top_left + (Vector2.One * 3) + (Vector2.UnitX * 2f), new Vector2((bottom_section_size.X) - 9, bottom_section_size.Y - 8f)));

            var tl = Drawing.measure_string_profont("x") ;
            ui.add_element("exit_button", new Button("x", resolution.X_only() - tl.X_only() - (Vector2.UnitX * 10f)));
            ui.add_element("minimize_button", new Button("_", resolution.X_only() - ((tl.X_only() + (Vector2.UnitX * 10f)) * 2), ui.elements["exit_button"].size));
            ((Button)ui.elements["exit_button"]).click_action = () => {
                Input.kill();
                this.Exit();
            };
            ((Button)ui.elements["minimize_button"]).click_action = () => {
                UIExterns.minimize_window();
            };

            ui.add_element("title_bar", new TitleBar(Vector2.Zero, new Vector2(resolution.X - ((ui.elements["exit_button"].width*2)), top_bar_height)));

            ui.add_element("mouse_delta", new MouseDeltaDisplay(
                mouse_position + (mouse_size / 2f) + Vector2.UnitX + (Vector2.UnitY * 5f), Vector2.One * 60f));

            ui.add_element("method_switch", new Button("RawInput", Vector2.UnitX * 100));
            ((Button)ui.elements["method_switch"]).color_foreground = Color.LightBlue;
            ((Button)ui.elements["method_switch"]).click_action = () => {
                //switch to monogame
                if (Input.poll_method == Input.input_method.RawInput) {
                    Input.change_polling_method(Input.input_method.MonoGame);
                    ((Button)ui.elements["method_switch"]).change_text("MonoGame");
                    ((Button)ui.elements["method_switch"]).color_foreground = Color.MonoGameOrange;

                //switch to rawinput
                } else if (Input.poll_method == Input.input_method.MonoGame) {
                    Input.change_polling_method(Input.input_method.RawInput);
                    ((Button)ui.elements["method_switch"]).change_text("RawInput");
                    ((Button)ui.elements["method_switch"]).color_foreground = Color.LightBlue;
                }
            };
        }

        protected override void Update(GameTime gameTime) {
            StringBuilder sb = new StringBuilder();

            string title_text = $"{UIExterns.get_window_title()}";
            string FPS_text = $"~{Input.frame_rate} Hz poll/{fps.frame_rate} FPS draw";

            ((TitleBar)ui.elements["title_bar"]).left_text = title_text;
            ((TitleBar)ui.elements["title_bar"]).right_text = FPS_text;

            ui.update();
            fps.update(gameTime);
            base.Update(gameTime);
        }

        static Vector2 top_section_size = new Vector2(850, 200);
        static Vector2 bottom_section_top_left => new Vector2(0, top_section_size.Y);
        static Vector2 bottom_section_size => new Vector2(resolution.X, resolution.Y - top_section_size.Y);

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            ui.draw();

            Drawing.rect(Vector2.UnitX, (Vector2.UnitY * top_bar_height) + (Vector2.UnitX * resolution.X), Color.White, 1f);
            Drawing.rect(Vector2.UnitX, top_section_size - Vector2.UnitY, Color.White, 1f);
            Drawing.rect(Vector2.Zero, resolution, Color.White, 2f);

            draw_keyboard();
            draw_mouse();

            Drawing.text($"[{Input.RAWINPUT_DEBUG_STRING}]", Vector2.UnitX * 200 + (Vector2.One * 3f), Color.White);

            Drawing.end();
            base.Draw(gameTime);
        }

        //EVERYTHING BELOW THIS IS KEYBOARD/MOUSE DRAWING CODE

        static float top_bar_height = 16f;

        Vector2 client_bounds_top_left => Vector2.UnitY * top_bar_height;
        Vector2 client_bounds_size => (resolution - client_bounds_top_left);

        //keyboard drawing
        //lomarf this should be good

        Vector2 keyboard_position => client_bounds_top_left + (Vector2.One * 7f);

        //SIZES
        Vector2 top_row_height => Vector2.UnitY * (base_key_size.Y + 6);

        Vector2 base_key_size => new Vector2(30, 26);
        Vector2 key_width => base_key_size.X_only();
        Vector2 key_height => base_key_size.Y_only();
        
        Vector2 tab_size => base_key_size + (key_width * 0.6f);
        Vector2 caps_size => base_key_size + (key_width * 0.8f);
        Vector2 left_shift_size => base_key_size + (key_width * 1.3f);

        Vector2 backslash_size => base_key_size + (key_width * .3f);
        Vector2 backspace_size => base_key_size + (key_width * .9f);
        Vector2 enter_size => base_key_size + (key_width * 1.2f);
        Vector2 right_shift_size => base_key_size + (key_width * 1.77f);

        Vector2 ctrl_size => base_key_size + (key_width * .47f);
        Vector2 alt_size => base_key_size + (key_width * .2f);
        Vector2 space_size => base_key_size + (key_width * 6);

        //GAPS
        Vector2 key_gap_x = Vector2.UnitX * 2;
        Vector2 key_gap_y = Vector2.UnitY * 2;
        Vector2 section_gap => (key_width * 0.2f);
        Vector2 f_key_gap => key_width * 0.69f;

        //ROW OFFSETS
        Vector2 top_row_top_left => keyboard_position;
        
        Vector2 second_row_top_left => top_row_top_left + top_row_height;

        Vector2 third_row_top_left => top_row_top_left + top_row_height + key_height + key_gap_y;
        Vector2 tab_top_right => third_row_top_left + (Vector2.UnitX * tab_size.X);

        Vector2 fourth_row_top_left => third_row_top_left + (key_height) + (key_gap_y);
        Vector2 caps_top_right => fourth_row_top_left + (Vector2.UnitX * caps_size.X);

        Vector2 fifth_row_top_left => fourth_row_top_left + (key_height) + (key_gap_y);
        Vector2 shift_top_right => fifth_row_top_left + (Vector2.UnitX * left_shift_size.X);
        Vector2 bottom_row => fifth_row_top_left + (key_height) + (key_gap_y);

        Vector2 up_arrow => shift_top_right + (key_gap_x * 14) + (key_width * 11) + right_shift_size.X_only() + section_gap;
        Vector2 down_arrow => up_arrow + base_key_size.Y_only() + (key_gap_y * 1);
        Vector2 left_arrow => up_arrow + base_key_size.Y_only() - base_key_size.X_only() + (key_gap_y * 1) - (key_gap_x * 1);
        Vector2 right_arrow => up_arrow + base_key_size.Y_only() + base_key_size.X_only() + (key_gap_y * 1) + (key_gap_x * 1);

        //SECTION OFFSETS
        Vector2 prtscr_section_top_left => top_row_top_left + (key_width * 16) + section_gap;
        Vector2 homeend_section_top_left => prtscr_section_top_left + top_row_height;
        Vector2 numpad_section_top_left => homeend_section_top_left + ((key_width + key_gap_x) * 3) + section_gap + Vector2.UnitX;
        Vector2 numpad_section_top_right => numpad_section_top_left + ((key_width + key_gap_x) * 4) + section_gap;

        enum align_text { left, right, center }

        void draw_input_key(Keys key, Vector2 position) { draw_input_key(key, "", key.ToString(), position, base_key_size, align_text.center); }
        void draw_input_key(Keys key, string key_string, Vector2 position) { draw_input_key(key, "", key_string, position, base_key_size, align_text.center); }
        void draw_input_key(Keys key, string key_string, Vector2 position, Vector2 size) { draw_input_key(key, "", key_string, position, size, align_text.center); }
        void draw_input_key(Keys key, string key_string, Vector2 position, Vector2 size, align_text alignment) { draw_input_key(key, "", key_string, position, size, alignment); }
        void draw_input_key(Keys key, string key_string_top, string key_string_bottom, Vector2 position) { draw_input_key(key, key_string_top, key_string_bottom, position, align_text.center); }
        void draw_input_key(Keys key, string key_string_top, string key_string_bottom, Vector2 position, align_text alignment) { draw_input_key(key, key_string_top, key_string_bottom, position, base_key_size, alignment); }
        void draw_input_key(Keys key, string key_string_top, string key_string_bottom, Vector2 position, Vector2 size) { draw_input_key(key, key_string_top, key_string_bottom, position, size, align_text.center); }
        void draw_input_key(Keys key, string key_string_top, string key_string_bottom, Vector2 position, Vector2 size, align_text alignment) {
            bool invert_text = false;

            if (Input.poll_method == Input.input_method.MonoGame) {
                if (Input.keyboard_state.IsKeyDown(key)) {
                    Drawing.fill_rect(position, size.X, size.Y, Color.MonoGameOrange);
                    invert_text = true;
                }
            } else if (Input.poll_method == Input.input_method.RawInput) {
                if (Input.ri_keyboard_state.IsKeyDown(key)) {
                    Drawing.fill_rect(position, size.X, size.Y, Color.LightBlue);
                    invert_text = true;
                }
            }

            Drawing.rect(position, size.X, size.Y, Color.White, 1f);

            var text_pos_top = Vector2.Zero;
            var text_pos_bottom = Vector2.Zero;

            var kst = Drawing.measure_string_profont(key_string_top);
            var ksb = Drawing.measure_string_profont(key_string_bottom);

            switch (alignment) {
                case align_text.left:
                    text_pos_top = position + (Vector2.UnitY * 3) + (Vector2.UnitX * 3);
                    text_pos_bottom = (position + size.Y_only()) - (ksb.Y_only()) + (Vector2.UnitX * 3);
                    break;

                case align_text.center:
                    text_pos_top = position + (size / 2f).X_only() - (Drawing.measure_string_profont(key_string_top).X_only() / 2f) + (Vector2.UnitY * 3);
                    text_pos_bottom = (position + (size - (size.X_only() / 2f))) - (ksb.X_only() / 2f) - (ksb.Y_only());
                    break;

                case align_text.right:
                    text_pos_top = (position + (Vector2.UnitX * size.X)) - kst.X_only() - (Vector2.UnitX * 3) + (Vector2.UnitY * 3);
                    text_pos_bottom = (position + size) - ksb - (Vector2.UnitX * 3);
                    break;
            }

            Drawing.text(key_string_top,
                text_pos_top,
                (invert_text ? Color.Black : Color.White));

            Drawing.text(key_string_bottom,
                text_pos_bottom,
                (invert_text ? Color.Black : Color.White));
        }


        //you ever just write something and know you don't need to write it but you know it's kinda neat and could be vaguely useful in the future        
        //ye
        //at the very least this could make for a great input display/keyboard cleaning & testing tool

        void draw_keyboard() {
            var n = 0; var c = 0; var g = 0;

            //Top row
            draw_input_key(Keys.Escape,           "esc",          top_row_top_left); c++;
            n++;
            draw_input_key(Keys.F1,                               top_row_top_left + (f_key_gap * n) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F2,                               top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F3,                               top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F4,                               top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            n++;
            draw_input_key(Keys.F5,                               top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F6,                               top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F7,                               top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F8,                               top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            n++;
            draw_input_key(Keys.F9,                               top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F10,                              top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F11,                              top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;
            draw_input_key(Keys.F12,                              top_row_top_left + (f_key_gap * n) + (key_gap_x * g) + (key_width * c)); c++; g++;

            n = 0;
            draw_input_key(Keys.PrintScreen,      "prt", "scr",   prtscr_section_top_left); n++;
            draw_input_key(Keys.Scroll,           "scl", "lck",   prtscr_section_top_left + (key_gap_x * n) + (key_width * 1)); n++;
            draw_input_key(Keys.Pause,            "pse", "brk",   prtscr_section_top_left + (key_gap_x * n) + (key_width * 2)); n++;

            //Second row
            n = 0;
            draw_input_key(Keys.OemTilde,         "~", "`",       second_row_top_left); n++;
            draw_input_key(Keys.D1,               "!", "1",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D2,               "@", "2",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D3,               "#", "3",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D4,               "$", "4",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D5,               "%", "5",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D6,               "^", "6",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D7,               "&", "7",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D8,               "*", "8",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D9,               "(", "9",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.D0,               ")", "0",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.OemMinus,         "_", "-",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.OemPlus,          "+", "=",       second_row_top_left + (key_gap_x * n) + (key_width * n)); n++;                                                                  
            draw_input_key(Keys.Back,             "bckspc",       second_row_top_left + (key_gap_x * n) + (key_width * n), backspace_size, align_text.right);

            //ins/del/etc block
            n = 0;
            draw_input_key(Keys.Insert,           "ins", "ert",   homeend_section_top_left); n++;
            draw_input_key(Keys.Home,             "home", "",     homeend_section_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.PageUp,           "pg", "up",     homeend_section_top_left + (key_gap_x * n) + (key_width * n)); n++;
            n = 0;                                                
            draw_input_key(Keys.Delete,           "del", "ete",   homeend_section_top_left + (key_gap_y + key_height)); n++;
            draw_input_key(Keys.End,              "end", "",      homeend_section_top_left + (key_gap_y + key_height) + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.PageDown,         "pg", "dn",     homeend_section_top_left + (key_gap_y + key_height) + (key_gap_x * n) + (key_width * n)); n++;

            //third row
            n = 0;
            draw_input_key(Keys.Tab,              "tab",          third_row_top_left, tab_size, align_text.left); n++;
            draw_input_key(Keys.Q,                                tab_top_right + key_gap_x); n++;
            draw_input_key(Keys.W,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.E,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.R,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.T,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.Y,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.U,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.I,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.O,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.P,                                tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.OemOpenBrackets,  "{", "[",       tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.OemCloseBrackets, "}", "]",       tab_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.OemPipe,          "|", "\\",      tab_top_right + (key_gap_x * n) + (key_width * (n-1)), backslash_size);

            //caps lock enabled bar
            if (Input.caps_lock) Drawing.line(fourth_row_top_left + Vector2.UnitY, fourth_row_top_left + Vector2.UnitY + caps_size.X_only(), Color.HotPink, 3f);

            //fourth row
            n = 0;                                
            draw_input_key(Keys.CapsLock,         "caps", "lock", fourth_row_top_left, caps_size, align_text.left); n++;
            draw_input_key(Keys.A,                                caps_top_right + key_gap_x); n++;
            draw_input_key(Keys.S,                                caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.D,                                caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.F,                                caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.G,                                caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.H,                                caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.J,                                caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.K,                                caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.L,                                caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.OemSemicolon,     ":", ";",       caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.OemQuotes,        "\"", "'",      caps_top_right + (key_gap_x * n) + (key_width * (n-1))); n++;
            draw_input_key(Keys.Enter,            "enter",        caps_top_right + (key_gap_x * n) + (key_width * (n-1)), enter_size, align_text.right);

            //fifth row
            n = 0;
            draw_input_key(Keys.LeftShift,        "shift",        fifth_row_top_left, left_shift_size, align_text.left); n++;
            draw_input_key(Keys.Z,                                shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.X,                                shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.C,                                shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.V,                                shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.B,                                shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.N,                                shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.M,                                shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.OemComma,         "<", ",",       shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.OemPeriod,        ">", ".",       shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.OemQuestion,      "?", "/",       shift_top_right + (key_gap_x * n) + (key_width * (n - 1))); n++;
            draw_input_key(Keys.RightShift,       "shift",        shift_top_right + (key_gap_x * n) + (key_width * (n - 1)), right_shift_size, align_text.right); n++;

            n = 0;
            //bottom row
            draw_input_key(Keys.LeftControl,      "ctrl",         bottom_row, ctrl_size, align_text.left); n++;
            draw_input_key(Keys.LeftWindows,      "win",          bottom_row + ctrl_size.X_only() + key_gap_x); n++;
            draw_input_key(Keys.LeftAlt,          "alt",          bottom_row + ctrl_size.X_only() + key_width + (key_gap_x * n), alt_size); n++;
            draw_input_key(Keys.Space,            "",             bottom_row + ctrl_size.X_only() + key_width + alt_size.X_only() + (key_gap_x * n), space_size); n++;
            draw_input_key(Keys.RightAlt,         "alt",          bottom_row + ctrl_size.X_only() + key_width + alt_size.X_only() + space_size.X_only() + (key_gap_x * n), alt_size); n++;
            draw_input_key(Keys.RightWindows,     "win",          bottom_row + ctrl_size.X_only() + key_width + alt_size.X_only() + space_size.X_only() + alt_size.X_only() + (key_gap_x * n)); n++;
            draw_input_key(Keys.Apps,             "menu",         bottom_row + ctrl_size.X_only() + key_width + alt_size.X_only() + space_size.X_only() + alt_size.X_only() + key_width + (key_gap_x * n)); n++;
            draw_input_key(Keys.RightControl,     "ctrl",         bottom_row + ctrl_size.X_only() + key_width + alt_size.X_only() + space_size.X_only() + alt_size.X_only() + key_width + key_width + (key_gap_x * n), ctrl_size, align_text.right); n++;


            //arrow keys
            draw_input_key(Keys.Up,               "",             up_arrow);
            draw_input_key(Keys.Down,             "",             down_arrow);
            draw_input_key(Keys.Left,             "",             left_arrow);
            draw_input_key(Keys.Right,            "",             right_arrow);

            //arrow key glyphs
            Drawing.image(tx_key_arrow, 
                up_arrow, base_key_size,
                Input.is_pressed(Keys.Up) ? Color.Black : Color.White);
            Drawing.image(tx_key_arrow, 
                down_arrow + Vector2.UnitX, base_key_size,
                Input.is_pressed(Keys.Down) ? Color.Black : Color.White, 
                180f);
            Drawing.image(tx_key_arrow, 
                left_arrow + (Vector2.UnitY * 3f), base_key_size,
                Input.is_pressed(Keys.Left) ? Color.Black : Color.White,
               -90f);
            Drawing.image(tx_key_arrow, 
                right_arrow + (Vector2.UnitY * 3f) + (Vector2.UnitX * 4f), base_key_size,
                Input.is_pressed(Keys.Right) ? Color.Black : Color.White,
                90f);

            //num lock enabled bar
            if (Input.num_lock) 
                Drawing.line(numpad_section_top_left + Vector2.UnitY, numpad_section_top_left + Vector2.UnitY + base_key_size.X_only(), Color.HotPink, 3f);

            //numpad
            n = 0;
            draw_input_key(Keys.NumLock,          "num", "lck",   numpad_section_top_left); n++;
            draw_input_key(Keys.Divide,           "/", "",        numpad_section_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.Multiply,         "*", "",        numpad_section_top_left + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.Subtract,         "-", "",        numpad_section_top_left + (key_gap_x * n) + (key_width * n)); n++;
            n = 0;
            draw_input_key(Keys.NumPad7,          "7", "home",    numpad_section_top_left + (key_height + key_gap_y)); n++;
            draw_input_key(Keys.NumPad8,          "8", "",        numpad_section_top_left + (key_height + key_gap_y) + (key_gap_x * n) + (key_width * n)); 
            Drawing.image(tx_key_arrow,
                numpad_section_top_left + (key_height + key_gap_y) + (key_gap_x * n) + (key_width * n) + (key_height * 0.4f), base_key_size,
                Input.is_pressed(Keys.NumPad8) ? Color.Black : Color.White); 
            n++;
            draw_input_key(Keys.NumPad9,          "9", "pgup",    numpad_section_top_left + (key_height + key_gap_y) + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.Add,              "+", "",        numpad_section_top_left + (key_height + key_gap_y) + (key_gap_x * n) + (key_width * n), base_key_size + (base_key_size.Y_only() + key_gap_y )); n++;
            n = 0;
            draw_input_key(Keys.NumPad4,          "4", "",        numpad_section_top_left + ((key_height + key_gap_y) * 2)); 
            Drawing.image(tx_key_arrow,
                numpad_section_top_left + ((key_height + key_gap_y) * 2) + (Vector2.UnitY * 3f) + (key_height * 0.15f) + (key_width * 0.15f), base_key_size,
                Input.is_pressed(Keys.NumPad4) ? Color.Black : Color.White,
               -90f); 
            n++;
            draw_input_key(Keys.NumPad5,          "5", "",        numpad_section_top_left + ((key_height + key_gap_y) * 2) + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.NumPad6,          "6", "",        numpad_section_top_left + ((key_height + key_gap_y) * 2) + (key_gap_x * n) + (key_width * n));
            Drawing.image(tx_key_arrow,
                numpad_section_top_left + ((key_height + key_gap_y) * 2) + (key_gap_x * n) + (key_width * n) + (Vector2.UnitY * 7f) - (Vector2.UnitX * 3f), base_key_size,
                Input.is_pressed(Keys.NumPad6) ? Color.Black : Color.White,
                90f); 
            n++;
            n = 0;
            draw_input_key(Keys.NumPad1,          "1", "end",     numpad_section_top_left + ((key_height + key_gap_y) * 3)); n++;
            draw_input_key(Keys.NumPad2,          "2", "",        numpad_section_top_left + ((key_height + key_gap_y) * 3) + (key_gap_x * n) + (key_width * n));
            Drawing.image(tx_key_arrow,
                numpad_section_top_left + ((key_height + key_gap_y) * 3) + (key_gap_x * n) + (key_width * n) + (key_height * 0.15f) + Vector2.UnitX, base_key_size,
                Input.is_pressed(Keys.NumPad2) ? Color.Black : Color.White,
                180f); 
            n++;
            draw_input_key(Keys.NumPad3,          "3", "pgdn",    numpad_section_top_left + ((key_height + key_gap_y) * 3) + (key_gap_x * n) + (key_width * n)); n++;
            draw_input_key(Keys.Enter,            "", "",         numpad_section_top_left + ((key_height + key_gap_y) * 3) + (key_gap_x * n) + (key_width * n), base_key_size + (base_key_size.Y_only() + key_gap_y)); n++;
            n = 0;
            draw_input_key(Keys.NumPad0,          "0", "ins",     numpad_section_top_left + ((key_height + key_gap_y) * 4), base_key_size + (base_key_size.X_only() + key_gap_x)); n++; n++;
            draw_input_key(Keys.Decimal,          ".", "del",     numpad_section_top_left + ((key_height + key_gap_y) * 4) + (key_gap_x * n) + (key_width * n)); n++;            
        }

        Vector2 mouse_position => numpad_section_top_right + (section_gap*2) - (base_key_size.Y_only());
        Vector2 mouse_size => new Vector2(91, 153);
        
        InputHandler mousehandler = new InputHandler();
        void draw_mouse() {
            mousehandler.update();
            Color c = Input.poll_method == Input.input_method.RawInput ? Color.LightBlue : Color.MonoGameOrange;

            if (Input.is_pressed(InputStructs.MouseButtons.Left))
                Drawing.image(tx_mouse_left, mouse_position, mouse_size, c);
            if (Input.is_pressed(InputStructs.MouseButtons.Right))
                Drawing.image(tx_mouse_right, mouse_position, mouse_size, c);
            if (Input.is_pressed(InputStructs.MouseButtons.Middle))
                Drawing.image(tx_mouse_middle, mouse_position, mouse_size, c);

            if (Input.is_pressed(InputStructs.MouseButtons.ScrollUp))
                Drawing.image(tx_mouse_scroll_up, mouse_position, mouse_size, c);
            if (Input.is_pressed(InputStructs.MouseButtons.ScrollDown))
                Drawing.image(tx_mouse_scroll_down, mouse_position, mouse_size, c);

            if (Input.is_pressed(InputStructs.MouseButtons.X1))
                Drawing.image(tx_mouse_xbutton1, mouse_position, mouse_size, c);
            if (Input.is_pressed(InputStructs.MouseButtons.X2))
                Drawing.image(tx_mouse_xbutton2, mouse_position, mouse_size, c);

            Drawing.image(tx_mouse_base, mouse_position, mouse_size);
        }

    }
}