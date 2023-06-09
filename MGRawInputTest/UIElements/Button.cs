﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;

namespace MGRawInputTest.UIElements { 
    public class Button : UIElement {
        string _text = "button";
        public string text => _text;
        Vector2 margin = (Vector2.UnitY * 2f) + (Vector2.UnitX * 5);

        public Action click_action = null;

        public Color color_foreground { get; set; } = Color.White;
        public Color color_background { get; set; } = Color.Black;

        public Button(string text, Vector2 position, Vector2 size) : base(position, size) {
            _text = text;
        }

        public Button(string text, Vector2 position) : base(position, Vector2.Zero) {
            _text = text;
            this.position = position;
            size = (margin * 2) + Drawing.measure_string_profont(text);
        }

        public void change_text(string text) {
            _text = text;
            size = (margin * 2) + Drawing.measure_string_profont(text);
        }

        public override void update() {
            //successful click, released left mouse while over the button and clicking
            if (!clicking && was_clicking && mouse_over) {
                if (click_action != null) click_action();
            }
        }


        public override void draw() { 
            bool col_toggle = mouse_over && !mouse_down;
            Drawing.fill_rect_outline(position, position + size, col_toggle ? color_foreground : color_background, color_foreground, 1f);
            Drawing.text(_text, position + margin, col_toggle ? color_background : color_foreground);
        }
        public override void draw_rt() { }
    }
}
