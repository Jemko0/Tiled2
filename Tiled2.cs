using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Tiled.Framework;
using Tiled2.Rendering;

namespace Tiled
{
    public class Tiled2 : Game
    {
        private GraphicsDeviceManager gdm;
        private SpriteBatch spriteBatch;
        public Renderer mainRenderer;

        public Tiled2()
        {
            gdm = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            gdm.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            GameState.Instance.Initialize();
            mainRenderer = new Renderer();
            mainRenderer.Init(ref spriteBatch, ref gdm, Content);
            mainRenderer.InitializeTileBuffers();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            System.Diagnostics.Debug.WriteLine("DELTA: " + delta);
            System.Diagnostics.Debug.WriteLine("FPS: " + 1.0f / delta);

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Rendering.Camera c = GameState.Instance.GetActiveCamera();
                c.position.Y -= 32 * delta;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Rendering.Camera c = GameState.Instance.GetActiveCamera();
                c.position.Y += 32 * delta;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Rendering.Camera c = GameState.Instance.GetActiveCamera();
                c.position.X -= 32 * delta;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Rendering.Camera c = GameState.Instance.GetActiveCamera();
                c.position.X += 32 * delta;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Add))
            {
                Rendering.Camera c = GameState.Instance.GetActiveCamera();
                c.zoom /= (1.0f - (1.0f * delta));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Subtract))
            {
                Rendering.Camera c = GameState.Instance.GetActiveCamera();
                c.zoom /= (1.0f + (1.0f * delta));
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);

            if (!GameState.Instance.initialized)
            {
                return;
            }

            mainRenderer.Render();
        }
    }
}
