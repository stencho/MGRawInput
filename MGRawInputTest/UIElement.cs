using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGRawInputTest {
    public abstract class UIElement {
        public Vector2 position { get; set; }
        public Vector2 size { get; set; }

        public float X => position.X;
        public float Y => position.Y;

        public float width => size.X;
        public float height => size.Y;

        internal bool mouse_over { get; set; } = false;
        internal bool mouse_down { get; set; } = false;
        internal bool mouse_was_down { get; set; } = false;
        internal bool right_mouse_down { get; set; } = false;
        internal bool right_mouse_was_down { get; set; } = false;

        internal bool clicking { get; set; } = false;
        internal bool was_clicking { get; set; } = false;

        bool _enable_rt = false;
        public bool enable_render_target {
            get {
                return _enable_rt;
            }
            set {
                if (_enable_rt == value) { 
                    return; 
                } else {
                    _enable_rt = value;

                    if (value) {
                        draw_target = new RenderTarget2D(Drawing.graphics_device, (int)width, (int)height);
                    } else {
                        draw_target = null;
                    }
                }
            }
        }
        internal RenderTarget2D draw_target;

        public UIElement(Vector2 position, Vector2 size) {
            this.position = position;
            this.size = size;
        }

        public abstract void update();
        public abstract void draw();
        public abstract void draw_rt();
        internal void base_update() {
            mouse_over = Collision2D.v2_intersects_rect(InputPolling.cursor_pos.ToVector2(), position, position + (size - Vector2.One));

            mouse_was_down = mouse_down;
            mouse_down = InputPolling.is_pressed(InputStructs.MouseButtons.Left);

            right_mouse_was_down = right_mouse_down;
            right_mouse_down = InputPolling.is_pressed(InputStructs.MouseButtons.Right);
            
            was_clicking = clicking;

            if (!UIExterns.in_foreground()) {
                clicking = false;
                return;
            }
            if (clicking && !mouse_down) clicking = false;
            if (mouse_over && mouse_down && !mouse_was_down) clicking = true;
        }
    }
}
