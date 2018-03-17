using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MultiplayerProject.Source
{
    public class MainGame : IScene
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private Player _player;

        private EnemyManager _enemyManager;
        private LaserManager _laserManager;
        private CollisionManager _collisionManager;
        private ExplosionManager _explosionManager;
        private BackgroundManager _backgroundManager;

        public MainGame(int width, int height)
        {
            Width = width;
            Height = height;
            
            _player = new Player(width, height);
            _enemyManager = new EnemyManager(width, height);
            _laserManager = new LaserManager(width, height);
            _backgroundManager = new BackgroundManager(width, height);

            _explosionManager = new ExplosionManager();
            _collisionManager = new CollisionManager();
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
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

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            // Thumbstick controls
            //_player.State.Position.X += _currentinputInfo.CurrentGamePadState.ThumbSticks.Left.X * _playerMoveSpeed;
            //_player.State.Position.Y -= _currentinputInfo.CurrentGamePadState.ThumbSticks.Left.Y * _playerMoveSpeed;

            // Keyboard/Dpad controls
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Left) || inputInfo.CurrentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                _player.RotateLeft(gameTime);
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Right) || inputInfo.CurrentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                _player.RotateRight(gameTime);
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Up) || inputInfo.CurrentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                _player.MoveForward(gameTime);
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Down) || inputInfo.CurrentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                _player.MoveBackward(gameTime);
            }

            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Space) || inputInfo.CurrentGamePadState.Buttons.X == ButtonState.Pressed)
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
