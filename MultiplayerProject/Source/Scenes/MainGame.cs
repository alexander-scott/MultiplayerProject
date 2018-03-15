using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class MainGame
    {
        private Player _player;

        private EnemyManager _enemyManager;
        private LaserManager _laserManager;
        private CollisionManager _collisionManager;
        private ExplosionManager _explosionManager;
        private BackgroundManager _backgroundManager;

        public MainGame(int width, int height)
        {
            _player = new Player(width, height);
            _enemyManager = new EnemyManager(width, height);
            _laserManager = new LaserManager(width, height);
            _backgroundManager = new BackgroundManager(width, height);

            _explosionManager = new ExplosionManager();
            _collisionManager = new CollisionManager();
        }

        public void Initalise(ContentManager content)
        {
            _player.Initialize(content);
            _enemyManager.Initalise(content);
            _laserManager.Initalise(content);
            _explosionManager.Initalise(content);
            _backgroundManager.Initalise(content);
        }

        public void Update(GameTime gameTime)
        {
            _player.Update(gameTime);
            _backgroundManager.Update(gameTime);
            _enemyManager.Update(gameTime);
            _laserManager.Update(gameTime);
            _explosionManager.Update(gameTime);

            _collisionManager.CheckCollision(_enemyManager.Enemies, _laserManager.Lasers, _explosionManager);
        }

        public void ProcessInput(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState, MouseState mouseState)
        {
            // Thumbstick controls
            //_player.State.Position.X += _currentGamePadState.ThumbSticks.Left.X * _playerMoveSpeed;
            //_player.State.Position.Y -= _currentGamePadState.ThumbSticks.Left.Y * _playerMoveSpeed;

            // Keyboard/Dpad controls
            if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.DPad.Left == ButtonState.Pressed)
            {
                _player.RotateLeft(gameTime);
            }
            if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.DPad.Right == ButtonState.Pressed)
            {
                _player.RotateRight(gameTime);
            }
            if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.DPad.Up == ButtonState.Pressed)
            {
                _player.MoveForward(gameTime);
            }
            if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.DPad.Down == ButtonState.Pressed)
            {
                _player.MoveBackward(gameTime);
            }

            if (keyboardState.IsKeyDown(Keys.Space) || gamePadState.Buttons.X == ButtonState.Pressed)
            {
                _laserManager.FireLaser(gameTime, _player.Position, _player.Rotation);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _backgroundManager.Draw(spriteBatch);

            _enemyManager.Draw(spriteBatch);

            _laserManager.Draw(spriteBatch);

            _explosionManager.Draw(spriteBatch);

            //_collisionManager.Draw(_graphics.GraphicsDevice, spriteBatch, _enemyManager.Enemies, _laserManager.Lasers);

            _player.Draw(spriteBatch);
        }
    }
}
