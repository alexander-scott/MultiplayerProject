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
        public int LastSequenceNumberProcessed { get; set; }
        public KeyboardMovementInput LastKeyboardMovementInput { get; set; }

        public struct ObjectState
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float Speed;
        }

        // Animation representing the player
        private Animation PlayerAnimation;

        // This is the player state that is drawn onto the screen. It is gradually
        // interpolated from the previousState toward the simultationState, in
        // order to smooth out any sudden jumps caused by discontinuities when
        // a network packet suddenly modifies the simultationState.
        protected ObjectState PlayerState;

        public Player()
        {
            PlayerState.Position = Vector2.Zero;
            PlayerState.Velocity = Vector2.Zero;
            PlayerState.Rotation = 0;

            // Set the player to be active
            Active = true;

            // Set the player health
            Health = Application.PLAYER_STARTING_HEALTH;
        }

        public void Initialize(ContentManager content, PlayerColour colour)
        {
            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = content.Load<Texture2D>("shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 0, 115, 69, 8, 30, new Color(colour.R, colour.G, colour.B), 1f, true);

            PlayerAnimation = playerAnimation;
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            if (PlayerAnimation != null)
            {
                PlayerAnimation.Position = PlayerState.Position;
                PlayerAnimation.Rotation = PlayerState.Rotation;

                PlayerAnimation.Update(gameTime);
            }
        }

        public void Update(float deltaTime)
        {
            Update(ref PlayerState, deltaTime);
        }

        public void Update(ref ObjectState state, float deltaTime)
        {
            // Limit the max speed
            if (state.Speed > Application.PLAYER_MAX_SPEED)
                state.Speed = Application.PLAYER_MAX_SPEED;
            else if (state.Speed < -Application.PLAYER_MAX_SPEED)
                state.Speed = -Application.PLAYER_MAX_SPEED;

            Vector2 direction = new Vector2((float)Math.Cos(state.Rotation),
                        (float)Math.Sin(state.Rotation));
            direction.Normalize();

            state.Position += direction * state.Speed;
            state.Speed *= Application.PLAYER_DECELERATION_AMOUNT;

            // Make sure that the player does not go out of bounds
            state.Position.X = MathHelper.Clamp(state.Position.X, 0, Application.WINDOW_WIDTH);
            state.Position.Y = MathHelper.Clamp(state.Position.Y, 0, Application.WINDOW_HEIGHT);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PlayerAnimation.Draw(spriteBatch);
        }

        public void SetPlayerState(PlayerUpdatePacket packet)
        {
            PlayerState.Position = new Vector2(packet.XPosition, packet.YPosition);
            PlayerState.Rotation = packet.Rotation;
            PlayerState.Speed = packet.Speed;
            // VELOCITY????/
        }

        public void ApplyInputToPlayer(KeyboardMovementInput input, float deltaTime)
        {
            ApplyInputToPlayer(ref PlayerState, input, deltaTime);
        }

        public void ApplyInputToPlayer(ref ObjectState state, KeyboardMovementInput input, float deltaTime)
        {
            if (input.DownPressed)
            {
                state.Speed -= Application.PLAYER_ACCELERATION_SPEED * deltaTime;
            }

            if (input.UpPressed)
            {
                state.Speed += Application.PLAYER_ACCELERATION_SPEED * deltaTime;
            }

            if (input.LeftPressed)
            {
                state.Rotation -= Application.PLAYER_ROTATION_SPEED * deltaTime;
            }

            if (input.RightPressed)
            {
                state.Rotation += Application.PLAYER_ROTATION_SPEED * deltaTime;
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