using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Ocean
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        Camera camera;
        SkyDome sky;
        LensFlareComponent lensFlare;
        Ocean ocean;

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        MouseState currentMouseState;
        Vector2 screenCenter;

        Vector3 lightDirection = new Vector3(1.0f, 0.0f, -1.0f);

        const bool isFullScreen = true;

        #region camera
        float cameraSpeed = 30;
        float YrotationSpeed;
        float XrotatiobSpeed;
        Vector3 position = new Vector3(-60.384f, 500.657f, 124.2985f);
        float yaw = -0.70341826f;
        float pitch = -0.0441565f;
        #endregion


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            if (!isFullScreen)
            {
                graphics.PreferredBackBufferWidth = 1200;
                graphics.PreferredBackBufferHeight = 800;
            }
            else
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.IsFullScreen = true;
            }

            graphics.PreferMultiSampling = true;
        }

        protected override void LoadContent()
        {
            lightDirection.Normalize();


            YrotationSpeed = (float)graphics.PreferredBackBufferWidth / 100000.0f;
            XrotatiobSpeed = (float)graphics.PreferredBackBufferHeight / 120000.0f;


            camera = new Camera(graphics.GraphicsDevice);
            ocean = new Ocean(Content.Load<Model>("Models/ocean"), Content.Load<Effect>("Effects/SimpleOceanShader"), Content.Load<Texture2D>("Textures/water"), Content.Load<Texture2D>("Textures/wavesbump"), lightDirection);
            sky = new SkyDome(Content.Load<Model>("Models/dome"), Content.Load<Texture2D>("Textures/SkyDay"));
            lensFlare = new LensFlareComponent(new SpriteBatch(graphics.GraphicsDevice), Content, graphics.GraphicsDevice);


            screenCenter = new Vector2(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height) / 2;
        }

        protected override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            camera.UpdateCamera(yaw, pitch, position);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(new Vector3(0.05f)));

            ocean.DrawOcean(gameTime, graphics, position, camera);
            sky.DrawDome(position, camera.view, camera.projection);
            lensFlare.Draw(camera.view, camera.projection, lightDirection, graphics.GraphicsDevice, Window);

            base.Draw(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();


            #region keyboard
            Vector3 moveVector = new Vector3(0, 0, 0);

            if (currentKeyboardState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (currentKeyboardState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (currentKeyboardState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (currentKeyboardState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            //if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
            //    moveVector += new Vector3(0, 1, 0);
            //if (currentKeyboardState.IsKeyDown(Keys.Space))
            //    moveVector += new Vector3(0, -1, 0);

            moveVector *= (float)gameTime.ElapsedGameTime.TotalMilliseconds / 100.0f;

            Matrix cameraRotation = /* Matrix.CreateRotationX(pitch) */ Matrix.CreateRotationY(yaw);
            Vector3 rotatedVector = Vector3.Transform(moveVector, cameraRotation);
            position += cameraSpeed * rotatedVector;

            //if (position.Y < 5f) position.Y = 5f;
            //if (position.Y > 3000f) position.Y = 3000f;
            #endregion


            #region mouse
            float amount = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            if (currentMouseState.X != screenCenter.X)
            {
                yaw -= YrotationSpeed * (currentMouseState.X - screenCenter.X) * amount;
            }
            if (currentMouseState.Y != screenCenter.Y)
            {
                pitch -= XrotatiobSpeed * (currentMouseState.Y - screenCenter.Y) * amount;
                if (pitch > MathHelper.ToRadians(90))
                    pitch = MathHelper.ToRadians(90);
                if (pitch < MathHelper.ToRadians(-90))
                    pitch = MathHelper.ToRadians(-90);
            }
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
            #endregion


            if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();
        }
    }



    #region Entery Point
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif

    #endregion
}
