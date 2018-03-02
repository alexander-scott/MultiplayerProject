using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    class Player
    {
        // Animation representing the player
        public Animation PlayerAnimation;

        // Position of the Player relative to the upper left corner of the screen
        public Vector2 Position;

        // State of the player
        public bool Active;

        // Amount of hit points that player has
        public int Health;

        // Get the width of the player ship
        public int Width
        {
            get { return PlayerAnimation.FrameWidth; }
        }

        // Get the height of the player ship
        public int Height
        {
            get { return PlayerAnimation.FrameHeight; }
        }

        private int _height;
        private int _width;

        public Player(int height, int width)
        {
            _height = height;
            _width = width;
        }

        public void Initialize(ContentManager content, Vector2 position)
        {
            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = content.Load<Texture2D>("shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

            PlayerAnimation = playerAnimation;

            // Set the starting position of the player around the middle of the screen and to the back
            Position = position;

            // Set the player to be active
            Active = true;

            // Set the player health
            Health = 100;
        }

        public void Update(GameTime gameTime)
        {
            PlayerAnimation.Position = Position;

            // Make sure that the player does not go out of bounds
            Position.X = MathHelper.Clamp(Position.X, 0, _height);
            Position.Y = MathHelper.Clamp(Position.Y, 0, _width);

            PlayerAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PlayerAnimation.Draw(spriteBatch);
        }
    }
}
