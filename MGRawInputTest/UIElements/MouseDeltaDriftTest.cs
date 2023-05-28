using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGRawInputTest.UIElements {
    public class MouseDeltaDriftTest : UIElement {
        Vector2 center => (size * 0.5f);

        Vector2 MonoGame_test_mouse_pos;
        Vector2 RawInput_test_mouse_pos;

        public MouseDeltaDriftTest(Vector2 position, Vector2 size) : base(position, size) {
            MonoGame_test_mouse_pos = center;
            RawInput_test_mouse_pos = center;
            enable_render_target = true;
        }

        public override void update() {            
            if (clicking) {
                //RESTORE THIS
                //MonoGame_test_mouse_pos += RawInputTest.input.mouse_delta_integer.ToVector2() * 10;
            } 
            if (!mouse_down) {
                MonoGame_test_mouse_pos = Vector2.LerpPrecise(MonoGame_test_mouse_pos, center, 25f * (float)(1000.0 / RawInputTest.target_fps / 1000.0));
                RawInput_test_mouse_pos = Vector2.LerpPrecise(RawInput_test_mouse_pos, center, 25f * (float)(1000.0 / RawInputTest.target_fps / 1000.0));
            }
        }

        public override void draw_rt() {
            Drawing.graphics_device.Clear(Color.Black);
            //SDF.draw_centered(Drawing.sdf_circle, center, Vector2.One * 7f, Color.White);
            SDF.draw_centered(Drawing.sdf_circle, MonoGame_test_mouse_pos, Vector2.One * 7f, Color.MonoGameOrange);
            SDF.draw_centered(Drawing.sdf_circle, RawInput_test_mouse_pos, Vector2.One * 7f, Color.LightBlue);
            if (clicking) {
                if (!was_clicking) 
                    InputPolling.hide_mouse();

                //RESTORE THIS
                //SDF.draw_centered(Drawing.sdf_circle, RawInputTest.input_draw.mouse_position - position, Vector2.One * 7f, Color.Transparent, Color.HotPink, 0.99f, 0.4f, 1f, false);
            } 
            
            if (!mouse_down && mouse_was_down) { 
                InputPolling.show_mouse();
            }
        }

        public override void draw() {
            Drawing.image(draw_target, position, size);
            Drawing.rect(position, position + size, Color.White, 1f);
            Drawing.text("MonoGame", position + Vector2.One * 3f, Color.MonoGameOrange);
            Drawing.text("RawInput", position + Vector2.One * 3f + (Drawing.measure_string_profont("MonoGame vs ").X_only()), Color.LightBlue);
            Drawing.text(" vs          mouse delta * 10", position + Vector2.One * 3f + (Drawing.measure_string_profont("MonoGame").X_only()), Color.White);            
        }

    }
}
