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
        public int LastSequenceNumberProcessed { get; set; }

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

        const int PLAYER_STARTING_HEALTH = 100;
        const float PLAYER_ACCELERATION_SPEED = 12f;
        const float PLAYER_ROTATION_SPEED = 2f;
        const float PLAYER_MAX_SPEED = 15f;
        const float PLAYER_DECELERATION_AMOUNT = 0.95f;

        public Player()
        {
            PlayerState.Position = Vector2.Zero;
            PlayerState.Velocity = Vector2.Zero;
            PlayerState.Rotation = 0;

            // Set the player to be active
            Active = true;

            // Set the player health
            Health = PLAYER_STARTING_HEALTH;
        }

        public void Initialize(ContentManager content)
        {
            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = content.Load<Texture2D>("shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 0, 115, 69, 8, 30, Color.White, 1f, true);

            PlayerAnimation = playerAnimation;
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

            // Make sure that the player does not go out of bounds
            PlayerState.Position.X = MathHelper.Clamp(PlayerState.Position.X, 0, Application.WINDOW_WIDTH);
            PlayerState.Position.Y = MathHelper.Clamp(PlayerState.Position.Y, 0, Application.WINDOW_HEIGHT);

            if (PlayerAnimation != null)
            {
                PlayerAnimation.Position = PlayerState.Position;
                PlayerAnimation.Rotation = PlayerState.Rotation;

                PlayerAnimation.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PlayerAnimation.Draw(spriteBatch);
        }

        public void SetObjectState(PlayerUpdatePacket packet)
        {
            PlayerState.Position = new Vector2(packet.XPosition, packet.YPosition);
            PlayerState.Rotation = packet.Rotation;
            PlayerState.Speed = packet.Speed;
            // VELOCITY????/
        }

        public void SetObjectStateLocal(KeyboardMovementInput input, float deltaTime)
        {
            if (input.DownPressed)
            {
                PlayerState.Speed -= PLAYER_ACCELERATION_SPEED * deltaTime;
            }

            if (input.UpPressed)
            {
                PlayerState.Speed += PLAYER_ACCELERATION_SPEED * deltaTime;
            }

            if (input.LeftPressed)
            {
                PlayerState.Rotation -= PLAYER_ROTATION_SPEED * deltaTime;
            }

            if (input.RightPressed)
            {
                PlayerState.Rotation += PLAYER_ROTATION_SPEED * deltaTime;
            }
        }

        public void SetObjectStateRemote(KeyboardMovementInput input, GameTime gameTime)
        {
            if (input.DownPressed)
            {
                PlayerState.Speed -= PLAYER_ACCELERATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (input.UpPressed)
            {
                PlayerState.Speed += PLAYER_ACCELERATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (input.LeftPressed)
            {
                PlayerState.Rotation -= PLAYER_ROTATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (input.RightPressed)
            {
                PlayerState.Rotation += PLAYER_ROTATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public PlayerUpdatePacket BuildUpdatePacket()
        {
            Vector2 pos = new Vector2((float)Math.Round((decimal)PlayerState.Position.X, 1), (float)Math.Round((decimal)PlayerState.Position.Y, 1));
            float speed = (float)Math.Round((decimal)PlayerState.Speed, 1);
            float rot = (float)Math.Round((decimal)PlayerState.Rotation, 1);
            return new PlayerUpdatePacket(pos.X, pos.Y, speed, rot);
        }
    }
}
