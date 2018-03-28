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

        public Vector2 CentrePosition { get { return new Vector2(Position.X - Width/2, Position.Y - Height / 2); } }
        public int Width { get; set; }
        public int Height { get { return EnemyAnimation.FrameHeight; } }

        public string EnemyID { get; set; }

        const float ENEMY_MOVE_SPEED = 6f;

        const int ENEMY_STARTING_HEALTH = 10;
        const int ENEMY_DAMAGE = 10;
        const int ENEMY_DEATH_SCORE_INCREASE = 100;

        public Enemy()
        {
            EnemyID = Guid.NewGuid().ToString();
            Width = 47;
        }

        public Enemy(string ID)
        {
            EnemyID = ID;
        }

        public void Initialize(Animation animation, Vector2 position)
        {    
            EnemyAnimation = animation;

            Position = position;

            Active = true;

            Health = ENEMY_STARTING_HEALTH;

            Damage = ENEMY_DAMAGE;

            Value = ENEMY_DEATH_SCORE_INCREASE;
        }

        public void Update(GameTime gameTime)
        {
            Update();

            // Update the position of the Animation
            EnemyAnimation.Position = Position;

            // Update Animation
            EnemyAnimation.Update(gameTime);
        }

        public void Update()
        {
            // The enemy always moves to the left so decrement its x position
            Position.X -= ENEMY_MOVE_SPEED;

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
            if (EnemyAnimation != null)
            {
                // Draw the animation
                EnemyAnimation.Draw(spriteBatch);
            }
        }
    }
}
