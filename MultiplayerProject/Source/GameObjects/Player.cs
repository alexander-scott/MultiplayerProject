using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class Player : INetworkedObject
    {
        public bool Active;
        public int Health;

        public int Width { get { return PlayerAnimation.FrameWidth; } }
        public int Height { get { return PlayerAnimation.FrameHeight; } }

        public Vector2 Position { get { return PlayerState.Position; } }
        public float Rotation { get { return PlayerState.Rotation; } }

        public string NetworkID { get; set; }
        public bool IsLocal { get; set; }

        private struct ObjectState
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float Speed;
        }

        // Animation representing the player
        private Animation PlayerAnimation;

        private ObjectState PlayerState;

        private int _height;
        private int _width;

        const int PLAYER_STARTING_HEALTH = 100;
        const float PLAYER_ACCELERATION_SPEED = 12f;
        const float PLAYER_ROTATION_SPEED = 2f;
        const float PLAYER_MAX_SPEED = 15f;
        const float PLAYER_DECELERATION_AMOUNT = 0.95f;

        public Player(int height, int width)
        {
            _height = height;
            _width = width;
        }

        public void Initialize(ContentManager content)
        {
            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = content.Load<Texture2D>("shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 0, 115, 69, 8, 30, Color.White, 1f, true);

            PlayerAnimation = playerAnimation;

            PlayerState.Position = Vector2.Zero;
            PlayerState.Velocity = Vector2.Zero;
            PlayerState.Rotation = 0;

            // Set the player to be active
            Active = true;

            // Set the player health
            Health = PLAYER_STARTING_HEALTH;
        }

        public void Update(GameTime gameTime)
        {
            // Limit the max speed
            if (PlayerState.Speed > PLAYER_MAX_SPEED)
                PlayerState.Speed = PLAYER_MAX_SPEED;

            Vector2 direction = new Vector2((float)Math.Cos(PlayerState.Rotation),
                        (float)Math.Sin(PlayerState.Rotation));
            direction.Normalize();

            PlayerState.Position += direction * PlayerState.Speed;
            PlayerState.Speed *= PLAYER_DECELERATION_AMOUNT;

            PlayerAnimation.Position = PlayerState.Position;
            PlayerAnimation.Rotation = PlayerState.Rotation;

            // Make sure that the player does not go out of bounds
            PlayerState.Position.X = MathHelper.Clamp(PlayerState.Position.X, 0, _height);
            PlayerState.Position.Y = MathHelper.Clamp(PlayerState.Position.Y, 0, _width);

            PlayerAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PlayerAnimation.Draw(spriteBatch);
        }

        public void RotateLeft(GameTime gameTime)
        {
            PlayerState.Rotation -= PLAYER_ROTATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void RotateRight(GameTime gameTime)
        {
            PlayerState.Rotation += PLAYER_ROTATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void MoveForward(GameTime gameTime)
        {
            PlayerState.Speed += PLAYER_ACCELERATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void MoveBackward(GameTime gameTime)
        {
            PlayerState.Speed -= PLAYER_ACCELERATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
