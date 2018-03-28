using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    class EnemyManager
    {
        public List<Enemy> Enemies { get { return _enemies; } }

        private Texture2D _enemyTexture;

        private List<Enemy> _enemies;

        private TimeSpan _enemySpawnTime;
        private TimeSpan _previousSpawnTime;

        private Random _random;

        public EnemyManager()
        {
        }

        public void Initalise(ContentManager content)
        {
            _enemyTexture = content.Load<Texture2D>("mineAnimation");

            // Initialize the enemies list
            _enemies = new List<Enemy>();

            // Set the time keepers to zero
            _previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            _enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // Initialize our random number generator
            _random = new Random();
        }

        public void Update(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if (gameTime.TotalGameTime - _previousSpawnTime > _enemySpawnTime)
            {
                _previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
            }

            // Update the Enemies
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                _enemies[i].Update(gameTime);

                if (_enemies[i].Active == false)
                {
                    _enemies.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                _enemies[i].Draw(spriteBatch);
            }
        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(_enemyTexture, Vector2.Zero, 0, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(Application.WINDOW_WIDTH + _enemyTexture.Width / 2,
                _random.Next(100, Application.WINDOW_HEIGHT - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            _enemies.Add(enemy);
        }
    }
}
