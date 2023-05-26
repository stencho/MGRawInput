using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace MGRawInputTest.UIElements { 
    public class Button : UIElement {
        string text = "button";
        Vector2 margin = Vector2.One * 2f + Vector2.UnitX;

        public Action click_action = null;

        public Button(string text, Vector2 position, Vector2 size) : base(position, size) {
            this.text = text;
        }

        public Button(string text, Vector2 position) : base(position, Vector2.Zero) {
            this.text = text;
            this.position = position;
            size = margin * 2 + Drawing.measure_string_profont(text);
        }

        public override void update() {
            base_update();

            //successful click, released left mouse while over the button and clicking
            if (!clicking && was_clicking && mouse_over)
            {
                if (click_action != null) click_action();
            }
        }


        public override void draw() { 
            bool col_toggle = mouse_over && !mouse_down;
            Drawing.fill_rect_outline(position, position + size, col_toggle ? Color.White : Color.Black, Color.White, 1f);
            Drawing.text(text, position + margin + Vector2.One, col_toggle ? Color.Black : Color.White);
        }
        public override void draw_rt() { }
    }
}
