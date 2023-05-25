using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace MGRawInputTest.UIElements {
    internal class MouseDeltaUI {
        Vector2 position; Vector2 size;
        Vector2 base_delta;

        public MouseDeltaUI(Vector2 position, Vector2 size) {
            this.position = position;
            this.size = size;
        }
        
        public void update(InputManager input) {
            base_delta = input.mouse_delta_integer;
        }

        public void draw() {
            SDF.draw_centered(Drawing.sdf_circle, position, size, Color.Black, Color.White, 0.95f, 0.1f, 1f,false);
            Drawing.line_rounded_ends(position, position + (Vector2.Normalize(base_delta) * (size/2f)) * 0.9f, Color.White, 3f);
        }
    }
}
