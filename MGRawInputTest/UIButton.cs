using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace MGRawInputTest {
    public class UIButton {
        Vector2 position; Vector2 size;

        bool _mouse_over = false;
        bool _mouse_down = false;
        bool _mouse_was_down = false;
        bool clicking = false;

        string text = "button";
        Vector2 margin = Vector2.One * 2f + (Vector2.UnitX);

        public Action click_action = null;

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
            _mouse_over = Collision2D.v2_intersects_rect(RawInputTest.input.mouse_position, position, position + size);            

            _mouse_was_down = _mouse_down;
            _mouse_down = InputPolling.is_pressed(InputStructs.MouseButtons.Left);       
            
            //successful click, released left mouse while over the button and clicking
            if (clicking && _mouse_over && (!_mouse_down)) {
                clicking = false;

                if (click_action != null) click_action();               
            }

            //stop clicking but don't do anything if left mouse is released while not over the button
            if (clicking && !_mouse_over && (!_mouse_down)) clicking = false; 

            //start clicking
            if (_mouse_over && (_mouse_down && !_mouse_was_down)) clicking = true;
        }


        public void draw() {
            bool col_toggle = (_mouse_over && !_mouse_down);
            Drawing.fill_rect_outline(position, position + size, col_toggle ? Color.White : Color.Black, Color.White, 1f);
            Drawing.text(text, position + margin + (Vector2.One), col_toggle ? Color.Black : Color.White);
        }
    }
}
