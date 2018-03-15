using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
    public class MainMenu : IScene
    {
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        private int _width;
        private int _height;

        public MainMenu(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }

        public void Initalise(ContentManager content)
        {
            throw new NotImplementedException();
        }

        public void ProcessInput(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState, MouseState mouseState)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
