using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
     public class MainMenu : IScene
    {
        public static event SimpleDelegate OnServerStartRequested;
        public static event SimpleDelegate OnClientStartRequested;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        private int _width;
        private int _height;

        private SpriteFont _font;
        private string _message;

        public MainMenu(int width, int height)
        {
            _width = width;
            _height = height;

            _message = "Press S to be a server \nPress C to be a client";
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _message, new Vector2(161, 161), Color.Black);
        }

        public void Initalise(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Font");
        }

        public void ProcessInput(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState, MouseState mouseState)
        {
            if (keyboardState.IsKeyDown(Keys.S))
            {
                OnServerStartRequested("str");
            }
            else if (keyboardState.IsKeyDown(Keys.C))
            {
                OnClientStartRequested("str");
            }
        }

        public void Update(GameTime gameTime)
        {
           
        }

        public void SetMessage(string message)
        {
            _message = message;
        }
    }
}
