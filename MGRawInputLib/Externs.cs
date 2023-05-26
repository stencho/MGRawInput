using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MGRawInputLib {
    public class Externs {
        [DllImport("user32.dll", SetLastError = true)] public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")] public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);
        [DllImport("user32.dll")] public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")] static extern bool GetCursorPos(out Point lpPoint);

        public static Microsoft.Xna.Framework.Rectangle get_window_rect() {
            RECT rect; GetWindowRect(actual_window_handle, out rect);
            return new Microsoft.Xna.Framework.Rectangle(rect.Location.X, rect.Location.Y, rect.Size.Width, rect.Size.Height);
        }
        public static Microsoft.Xna.Framework.Point get_window_pos() {
            RECT rect; GetWindowRect(actual_window_handle, out rect);
            return new Microsoft.Xna.Framework.Point(rect.Location.X, rect.Location.Y);
        }
        public static Microsoft.Xna.Framework.Point get_window_size() {
            RECT rect; GetWindowRect(actual_window_handle, out rect);
            return new Microsoft.Xna.Framework.Point(rect.Size.Width, rect.Size.Height);
        }
        public static Microsoft.Xna.Framework.Point get_cursor_pos() {
            Point p; GetCursorPos(out p);
            return new Microsoft.Xna.Framework.Point(p.X, p.Y);
        }

        public static IntPtr actual_window_handle = current_process_monogame_window_handle();
        static IntPtr current_process_monogame_window_handle() {
            IntPtr current_window = IntPtr.Zero;
            int this_process_id = Process.GetCurrentProcess().Id;
            StringBuilder sb = new StringBuilder();
            do {
                current_window = FindWindowEx(IntPtr.Zero, current_window, null, null);
                int procid = 0; int threadid = GetWindowThreadProcessId(current_window, out procid);
                if (procid == this_process_id) {
                    sb.Clear(); GetClassName(current_window, sb, 20);
                    if (sb.ToString() == "SDL_app") {
                        return current_window;
                    }
                }
            } while (current_window != IntPtr.Zero);

            return IntPtr.Zero;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left_, int top_, int right_, int bottom_) {
                Left = left_;
                Top = top_;
                Right = right_;
                Bottom = bottom_;
            }

            public int Height { get { return Bottom - Top; } }
            public int Width { get { return Right - Left; } }
            public Size Size { get { return new Size(Width, Height); } }

            public Point Location { get { return new Point(Left, Top); } }

            // Handy method for converting to a System.Drawing.Rectangle
            public Rectangle ToRectangle() { return Rectangle.FromLTRB(Left, Top, Right, Bottom); }

            public static RECT FromRectangle(Rectangle rectangle) {
                return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            public override int GetHashCode() {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                  ^ ((Width << 0x1a) | (Width >> 6))
                  ^ ((Height << 7) | (Height >> 0x19));
            }

            #region Operator overloads

            public static implicit operator Rectangle(RECT rect) {
                return rect.ToRectangle();
            }

            public static implicit operator RECT(Rectangle rect) {
                return FromRectangle(rect);
            }

            #endregion
        }
    }
}