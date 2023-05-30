using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGRawInputLib;

namespace MGRawInputTest {
    
    internal class UIElementManager {
        public Dictionary<string, UIElement> elements = new Dictionary<string, UIElement>();
        public UIElement? focused_element = null;
        public void add_element(string name, UIElement element) {
            elements.Add(name, element);
        }

        bool mouse_down_prev = false;
        public void update() {
            bool hit = false;
            foreach(string k in elements.Keys) {                
                if (elements[k].click_update()) {
                    if (!hit) hit = true;

                    focused_element = elements[k];
                }

                bool mouse_down = Input.is_pressed(InputStructs.MouseButtons.Left);
                if (!hit && mouse_down && mouse_down_prev) {
                    focused_element = null;
                }

                elements[k].update();
                mouse_down_prev = mouse_down;
            }
        }
        public void draw() {
            foreach (string k in elements.Keys) {
                if (elements[k].enable_render_target) {
                    Drawing.graphics_device.SetRenderTarget(elements[k].draw_target);
                    elements[k].draw_rt();
                    Drawing.graphics_device.SetRenderTarget(null);
                    elements[k].draw(); 
                }
            }
            Drawing.graphics_device.SetRenderTarget(null);
            foreach (string k in elements.Keys) {
                if (!elements[k].enable_render_target) {
                    elements[k].draw();
                }
            }
        }
    }
}
