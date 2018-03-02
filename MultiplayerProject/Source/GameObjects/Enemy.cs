using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    class Enemy
    {
        public Animation EnemyAnimation;
        public Vector2 Position;
        public bool Active;

        public int Health;
        public int Damage; // The amount of damage the enemy inflicts on the player ship
        public int Value;  // The amount of score the enemy will give to the player

        public int Width { get { return EnemyAnimation.FrameWidth; } }
        public int Height { get { return EnemyAnimation.FrameHeight; } }

        private const float _enemyMoveSpeed = 6f;

        public void Initialize(Animation animation, Vector2 position)
        {    
            EnemyAnimation = animation;

            Position = position;

            Active = true;

            Health = 10;

            Damage = 10;

            Value = 100;
        }

        public void Update(GameTime gameTime)
        {
            // The enemy always moves to the left so decrement its x position
            Position.X -= _enemyMoveSpeed;

            // Update the position of the Animation
            EnemyAnimation.Position = Position;

            // Update Animation
            EnemyAnimation.Update(gameTime);

            // If the enemy is past the screen or its health reaches 0 then deactivate it
            if (Position.X < -Width || Health <= 0)
            {
                // By setting the Active flag to false, the game will remove this objet from the
                // active game list
                Active = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the animation
            EnemyAnimation.Draw(spriteBatch);
        }
    }
}
