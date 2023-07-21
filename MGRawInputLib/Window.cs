﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static MGRawInputLib.Externs;

namespace MGRawInputLib {
    public static class Window {
        static Point relative_mouse;

        public static Action<Point>? resize_start;
        public static Action<Point>? resize_end;

        public static Rectangle window_rect = Rectangle.Empty;

        public static void update() {
            window_rect = Externs.get_window_rect();
            var cp = Externs.get_cursor_pos();            

            if (moving_window && Input.is_pressed(InputStructs.MouseButtons.Left)) {
                if (Input.mouse_delta.X != 0 || Input.mouse_delta.Y != 0) {
                    Externs.MoveWindow(Externs.actual_window_handle,
                        cp.X - relative_mouse.X,
                        cp.Y - relative_mouse.Y,
                        window_rect.Width, window_rect.Height, false);
                }
            } else if (resizing_window && Input.is_pressed(InputStructs.MouseButtons.Left)) {
                if (Input.mouse_delta.X != 0 || Input.mouse_delta.Y != 0) {
                    Externs.MoveWindow(Externs.actual_window_handle,
                        window_rect.X, window_rect.Y,
                    start_size.X - (relative_mouse.X - (cp.X - window_rect.X)), 
                    start_size.Y - (relative_mouse.Y - (cp.Y - window_rect.Y)), false);
                }
            }
        }

        static Point start_size = Point.Zero;
        static bool _rz_wnd = false;
        public static bool resizing_window {
            get {
                return _rz_wnd;
            }
            set {
                if (_rz_wnd == false && value == true) {
                    relative_mouse = Externs.get_cursor_pos() - Externs.get_window_pos();
                    start_size = Externs.get_window_rect().Size;

                    if (resize_start != null) resize_start(Externs.get_window_rect().Size);
                }
                if (_rz_wnd == true && value == false) {
                    if (resize_end != null) resize_end(Externs.get_window_rect().Size);
                }
                _rz_wnd = value;
            }
        }

        static bool _mv_wnd = false;
        public static bool moving_window {
            get {
                return _mv_wnd;
            }
            set {
                if (_mv_wnd == false && value == true)
                    relative_mouse = Externs.get_cursor_pos() - Externs.get_window_pos();

                _mv_wnd = value;
            }
        }
    }
}
