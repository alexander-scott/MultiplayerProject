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

        private Random _random;
        private float _width;

        public EnemyManager()
        {
            // Initialize the enemies list
            _enemies = new List<Enemy>();

            // Initialize our random number generator
            _random = new Random();

            _width = 47;
        }

        public void Initalise(ContentManager content)
        {
            _enemyTexture = content.Load<Texture2D>("mineAnimation");
        }

        public void Update(GameTime gameTime)
        {
            // Update the Enemies
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i].EnemyAnimation == null)
                    _enemies[i].Update();
                else
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

        public Enemy AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(_enemyTexture, Vector2.Zero, 0, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(Application.WINDOW_WIDTH + _width / 2,
                _random.Next(100, Application.WINDOW_HEIGHT - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            _enemies.Add(enemy);

            return enemy;
        }

        public void AddEnemy(Vector2 position, string enemyID)
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(_enemyTexture, Vector2.Zero, 0, 47, 61, 8, 30, Color.White, 1f, true);

            // Create an enemy
            Enemy enemy = new Enemy(enemyID);

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            _enemies.Add(enemy);
        }

        public void DeactivateEnemy(string enemyID)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].EnemyID == enemyID)
                {
                    _enemies[i].Active = false;
                    return;
                }
            }
        }

        public Enemy DeactivateAndReturnEnemy(string enemyID)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].EnemyID == enemyID)
                {
                    _enemies[i].Active = false;
                    return _enemies[i];
                }
            }

            return null;
        }
    }
}
