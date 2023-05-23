using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGRawInputTest {
    public class RawInputTest : Game {
        private GraphicsDeviceManager graphics;

        public RawInputTest() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
            graphics.ApplyChanges();
        }

        protected override void Initialize() {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent() {
            Drawing.load(GraphicsDevice, graphics, Content);
            SDF.load(Content);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Drawing.fill_rect_outline(Vector2.One * 20f, Vector2.One * 150f, Color.Red, Color.Pink, 4f);
            Drawing.image(Drawing.sdf_circle, Vector2.One * 90f, Vector2.One * 160f);

            SDF.draw_centered(Drawing.sdf_circle, Vector2.One * 20f, Vector2.One * 130f, Color.White);

            Drawing.fill_circle(Vector2.One * 250f, 50f, Color.Pink);
            Drawing.circle(Vector2.One * 250f, 50f, 10f, Color.Red);
            Drawing.line_rounded_ends(Vector2.One * 150f, Vector2.One * 300f, Color.HotPink, 10f);
            Drawing.poly(Color.Red, 4f, true,
                Vector2.One * 50f, 
                (Vector2.One * 50f) + (Vector2.UnitY * 150f), 
                (Vector2.One * 50f) + (Vector2.UnitY * 150f) + (Vector2.UnitX * 150f),
                (Vector2.One * 50f) + (Vector2.UnitY * 150f) + (Vector2.UnitX * 150f) + (Vector2.UnitY * 150f),
                (Vector2.One * 50f) + (Vector2.UnitY * 150f) + (Vector2.UnitX * 150f) + (Vector2.UnitY * 150f) + (Vector2.UnitX * 250f)
                );

            Drawing.text_shadow("test haha", Vector2.UnitX * 100 + (Vector2.UnitY * 25), Color.White);

            Drawing.end();
            base.Draw(gameTime);
        }
    }
}