using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MGRawInputTest {
    public class UIButton {
        Vector2 position; Vector2 size;

        bool _mouse_over = false;
        bool _mouse_down = false;

        string text = "button";
        Vector2 margin = Vector2.One * 2f + (Vector2.UnitX);

        Action click_action = null;

        public UIButton(string text, Vector2 position, Vector2 size) {
            this.text = text;
            this.position = position;
            this.size = size;
        }

        public UIButton(string text, Vector2 position) {
            this.text = text;
            this.position = position;
            this.size = (margin * 2) + Drawing.measure_string_profont(text);
        }

        public void update() {

        }

        public void draw() {
            Drawing.fill_rect_outline(position, position + size, Color.Black, Color.White, 1f);
            Drawing.text(text, position + margin + (Vector2.One), Color.White);
        }
    }
}
