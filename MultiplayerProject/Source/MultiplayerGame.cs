using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using MultiplayerProject.Source;

namespace MultiplayerProject
{
    public class MultiplayerGame : Game
    {
        GraphicsDeviceManager   _graphics;
        SpriteBatch             _spriteBatch;
        Player                  _player;

        KeyboardState           _currentKeyboardState;
        KeyboardState           _previousKeyboardState;

        GamePadState            _currentGamePadState;
        GamePadState            _previousGamePadState;

        MouseState              _currentMouseState;
        MouseState              _previousMouseState;

        float                   _playerMoveSpeed;

        EnemyManager            _enemyManager;
        CollisionManager        _collisionManager;

        // Image used to display the static background
        Texture2D mainBackground;
        Rectangle rectBackground;

        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;

        List<Laser> laserBeams;
        // texture to hold the laser.
        Texture2D laserTexture;
        // govern how fast our laser can fire.
        TimeSpan laserSpawnTime;
        TimeSpan previousLaserSpawnTime;

        // Collections of explosions
        List<Explosion> explosions;

        //Texture to hold explosion animation.
        Texture2D explosionTexture;

        public MultiplayerGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // Initialize the player class
            _player = new Player();

            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();
            rectBackground = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            _playerMoveSpeed = 8.0f;
            TouchPanel.EnabledGestures = GestureType.FreeDrag;

            _enemyManager = new EnemyManager();
            _enemyManager.Initalise(Content, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            _collisionManager = new CollisionManager();

            // init our laser
            laserBeams = new List<Laser>();
            const float SECONDS_IN_MINUTE = 60f;
            const float RATE_OF_FIRE = 200f;
            laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / RATE_OF_FIRE);
            previousLaserSpawnTime = TimeSpan.Zero;

            // init our collection of explosions.
            explosions = new List<Explosion>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);

            _player.Initialize(playerAnimation, playerPosition);

            bgLayer1.Initialize(Content, "bgLayer1", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -1);
            bgLayer2.Initialize(Content, "bgLayer2", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -2);
            mainBackground = Content.Load<Texture2D>("mainbackground");

            // load th texture to serve as the laser
            laserTexture = Content.Load<Texture2D>("laser");

            // load the explosion sheet
            explosionTexture = Content.Load<Texture2D>("explosion");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _previousGamePadState = _currentGamePadState;
            _previousKeyboardState = _currentKeyboardState;
            _previousMouseState = _currentMouseState;

            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _currentMouseState = Mouse.GetState();

            UpdatePlayer(gameTime);

            // Update the parallaxing background
            bgLayer1.Update(gameTime);
            bgLayer2.Update(gameTime);

            _enemyManager.Update(gameTime);
            _collisionManager.CheckCollision(_enemyManager.Enemies, laserBeams);

            UpdateExplosions(gameTime);

            // update laserbeams
            for (var i = 0; i < laserBeams.Count; i++)
            {
                laserBeams[i].Update(gameTime);
                // Remove the beam when its deactivated or is at the end of the screen.
                if (!laserBeams[i].Active || laserBeams[i].Position.X > GraphicsDevice.Viewport.Width)
                {
                    AddExplosion(laserBeams[i].Position);
                    laserBeams.Remove(laserBeams[i]);
                }
            }

            base.Update(gameTime);
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            _player.Update(gameTime);

            // Windows 8 Touch Gestures for MonoGame
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                if (gesture.GestureType == GestureType.FreeDrag)
                {
                    _player.Position += gesture.Delta;
                }
            }

            //Get Mouse State then Capture the Button type and Respond Button Press
            Vector2 mousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);

            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 posDelta = mousePosition - _player.Position;

                posDelta.Normalize();
                posDelta = posDelta * _playerMoveSpeed;

                _player.Position = _player.Position + posDelta;
            }

            // Thumbstick controls
            _player.Position.X += _currentGamePadState.ThumbSticks.Left.X * _playerMoveSpeed;
            _player.Position.Y -= _currentGamePadState.ThumbSticks.Left.Y * _playerMoveSpeed;

            // Keyboard/Dpad controls
            if (_currentKeyboardState.IsKeyDown(Keys.Left) || _currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                _player.Position.X -= _playerMoveSpeed;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Right) || _currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                _player.Position.X += _playerMoveSpeed;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Up) || _currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                _player.Position.Y -= _playerMoveSpeed;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Down) || _currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                _player.Position.Y += _playerMoveSpeed;
            }

            if (_currentKeyboardState.IsKeyDown(Keys.Space) || _currentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                FireLaser(gameTime);
            }

            // Make sure that the player does not go out of bounds
            _player.Position.X = MathHelper.Clamp(_player.Position.X, 0, GraphicsDevice.Viewport.Width);
            _player.Position.Y = MathHelper.Clamp(_player.Position.Y, 0, GraphicsDevice.Viewport.Height);
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for (var e = 0; e < explosions.Count; e++)
            {
                explosions[e].Update(gameTime);

                if (!explosions[e].Active)
                    explosions.Remove(explosions[e]);
            }
        }

        protected void FireLaser(GameTime gameTime)
        {
            // govern the rate of fire for our lasers
            if (gameTime.TotalGameTime - previousLaserSpawnTime > laserSpawnTime)
            {
                previousLaserSpawnTime = gameTime.TotalGameTime;
                // Add the laer to our list.
                AddLaser();
            }
        }

        protected void AddLaser()
        {
            Animation laserAnimation = new Animation();
            // initlize the laser animation
            laserAnimation.Initialize(laserTexture,
                _player.Position,
                46,
                16,
                1,
                30,
                Color.White,
                1f,
                true);

            Laser laser = new Laser();
            // Get the starting postion of the laser.

            var laserPostion = _player.Position;
            // Adjust the position slightly to match the muzzle of the cannon.
            //laserPostion.Y += 37;
            laserPostion.X += 70;

            // init the laser
            laser.Initialize(laserAnimation, laserPostion);
            laserBeams.Add(laser);
            /* todo: add code to create a laser. */
            // laserSoundInstance.Play();
        }

        protected void AddExplosion(Vector2 enemyPosition)
        {
            Animation explosionAnimation = new Animation();

            explosionAnimation.Initialize(explosionTexture,
                enemyPosition,
                134,
                134,
                12,
                30,
                Color.White,
                1.0f,
                true);

            Explosion explosion = new Explosion();
            explosion.Initialize(explosionAnimation, enemyPosition);

            explosions.Add(explosion);
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            // Start drawing
            _spriteBatch.Begin();

            //Draw the Main Background Texture
            _spriteBatch.Draw(mainBackground, rectBackground, Color.White);

            // Draw the moving background
            bgLayer1.Draw(_spriteBatch);
            bgLayer2.Draw(_spriteBatch);

            _enemyManager.Draw(_spriteBatch);

            // Draw the lasers.
            foreach (var l in laserBeams)
            {
                l.Draw(_spriteBatch);
            }

            // draw explosions
            foreach (var e in explosions)
            {
                e.Draw(_spriteBatch);
            }

            // Draw the Player
            _player.Draw(_spriteBatch);

            // Stop drawing
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
