using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MultiplayerProject.Source;
using System;

namespace MultiplayerProject
{
    public class MultiplayerGame : Game
    {
        private GraphicsDeviceManager   _graphics;
        private SpriteBatch             _spriteBatch;
        private Player                  _player;

        private KeyboardState           _currentKeyboardState;
        private KeyboardState           _previousKeyboardState;

        private GamePadState            _currentGamePadState;
        private GamePadState            _previousGamePadState;

        private MouseState              _currentMouseState;
        private MouseState              _previousMouseState;

        private EnemyManager            _enemyManager;
        private LaserManager            _laserManager;
        private CollisionManager        _collisionManager;
        private ExplosionManager        _explosionManager;
        private BackgroundManager       _backgroundManager;

        public MultiplayerGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _player = new Player(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _enemyManager = new EnemyManager(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _laserManager = new LaserManager(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _backgroundManager = new BackgroundManager(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            _explosionManager = new ExplosionManager();
            _collisionManager = new CollisionManager(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _player.Initialize(Content);
            _enemyManager.Initalise(Content);
            _laserManager.Initalise(Content);
            _explosionManager.Initalise(Content);
            _backgroundManager.Initalise(Content);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            ProcessInput(gameTime);

            _player.Update(gameTime);
            _backgroundManager.Update(gameTime);
            _enemyManager.Update(gameTime);
            _laserManager.Update(gameTime);
            _explosionManager.Update(gameTime);

            _collisionManager.CheckCollision(_enemyManager.Enemies, _laserManager.Lasers, _explosionManager);

            base.Update(gameTime);
        }

        private void ProcessInput(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _previousGamePadState = _currentGamePadState;
            _previousKeyboardState = _currentKeyboardState;
            _previousMouseState = _currentMouseState;

            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _currentMouseState = Mouse.GetState();

            // Thumbstick controls
            //_player.State.Position.X += _currentGamePadState.ThumbSticks.Left.X * _playerMoveSpeed;
            //_player.State.Position.Y -= _currentGamePadState.ThumbSticks.Left.Y * _playerMoveSpeed;

            // Keyboard/Dpad controls
            if (_currentKeyboardState.IsKeyDown(Keys.Left) || _currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                _player.RotateLeft(gameTime);
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Right) || _currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                _player.RotateRight(gameTime);
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Up) || _currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                _player.MoveForward(gameTime);
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Down) || _currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                _player.MoveBackward(gameTime);
            }

            if (_currentKeyboardState.IsKeyDown(Keys.Space) || _currentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                _laserManager.FireLaser(gameTime, _player.Position, _player.Rotation);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            _spriteBatch.Begin();

            _backgroundManager.Draw(_spriteBatch);

            _enemyManager.Draw(_spriteBatch);

            _laserManager.Draw(_spriteBatch);

            _explosionManager.Draw(_spriteBatch);

            _collisionManager.Draw(_spriteBatch, _enemyManager.Enemies, _laserManager.Lasers);

            _player.Draw(_spriteBatch);

            // Stop drawing
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
