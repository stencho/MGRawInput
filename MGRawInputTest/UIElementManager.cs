using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGRawInputTest {
    
    internal class UIElementManager {
        public Dictionary<string, UIElement> elements = new Dictionary<string, UIElement>();

        public void add_element(string name, UIElement element) {
            elements.Add(name, element);
        }

        public void update() { 
            foreach(string k in elements.Keys) {
                elements[k].update();                
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
