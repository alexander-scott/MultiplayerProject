using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    class EnemyManager
    {
        public List<Enemy> Enemies
        {
            get { return enemies; }
        }

        // Enemies
        Texture2D enemyTexture;

        List<Enemy> enemies;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;

        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;

        private int _screenWidth;
        private int _screenHeight;

        public void Initalise(ContentManager content, int screenWidth, int screenHeight)
        {
            enemyTexture = content.Load<Texture2D>("mineAnimation");

            _screenWidth = screenWidth;
            _screenHeight = screenHeight;

            // Initialize the enemies list
            enemies = new List<Enemy>();

            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // Initialize our random number generator
            random = new Random();
        }

        public void Update(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
            }

            // Update the Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(gameTime);

                if (enemies[i].Active == false)
                {
                    enemies.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Draw(spriteBatch);
            }
        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(_screenWidth + enemyTexture.Width / 2,

            random.Next(100, _screenHeight - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);
        }
    }
}
