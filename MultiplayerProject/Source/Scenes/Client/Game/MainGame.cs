using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public class MainGame : IScene
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private List<Player> _players;

        private EnemyManager _enemyManager;
        private LaserManager _laserManager;
        private CollisionManager _collisionManager;
        private ExplosionManager _explosionManager;
        private BackgroundManager _backgroundManager;

        public MainGame(int width, int height, int playerCount, string[] playerIDs, string localPlayerID)
        {
            Width = width;
            Height = height;

            _players = new List<Player>();

            for (int i = 0; i < playerCount; i++)
            {
                Player player = new Player(width, height);
                player.NetworkID = playerIDs[i];

                if (playerIDs[i] == localPlayerID)
                    player.IsLocal = true;
                else
                    player.IsLocal = false;

                _players.Add(player);
            }
            
            _enemyManager = new EnemyManager(width, height);
            _laserManager = new LaserManager(width, height);
            _backgroundManager = new BackgroundManager(width, height);

            _explosionManager = new ExplosionManager();
            _collisionManager = new CollisionManager();
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                _players[i].Initialize(content);
            }

            _enemyManager.Initalise(content);
            _laserManager.Initalise(content);
            _explosionManager.Initalise(content);
            _backgroundManager.Initalise(content);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                _players[i].Update(gameTime);
            }

            _backgroundManager.Update(gameTime);
            _enemyManager.Update(gameTime);
            _laserManager.Update(gameTime);
            _explosionManager.Update(gameTime);

            _collisionManager.CheckCollision(_enemyManager.Enemies, _laserManager.Lasers, _explosionManager);
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].IsLocal)
                {
                    // Keyboard/Dpad controls
                    if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Left) || inputInfo.CurrentGamePadState.DPad.Left == ButtonState.Pressed)
                    {
                        _players[i].RotateLeft(gameTime);
                    }
                    if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Right) || inputInfo.CurrentGamePadState.DPad.Right == ButtonState.Pressed)
                    {
                        _players[i].RotateRight(gameTime);
                    }
                    if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Up) || inputInfo.CurrentGamePadState.DPad.Up == ButtonState.Pressed)
                    {
                        _players[i].MoveForward(gameTime);
                    }
                    if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Down) || inputInfo.CurrentGamePadState.DPad.Down == ButtonState.Pressed)
                    {
                        _players[i].MoveBackward(gameTime);
                    }

                    if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Space) || inputInfo.CurrentGamePadState.Buttons.X == ButtonState.Pressed)
                    {
                        _laserManager.FireLaser(gameTime, _players[i].Position, _players[i].Rotation);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _backgroundManager.Draw(spriteBatch);

            _enemyManager.Draw(spriteBatch);

            _laserManager.Draw(spriteBatch);

            _explosionManager.Draw(spriteBatch);

            //_collisionManager.Draw(_graphics.GraphicsDevice, spriteBatch, _enemyManager.Enemies, _laserManager.Lasers);

            for (int i = 0; i < _players.Count; i++)
            {
                _players[i].Draw(spriteBatch);
            }
        }        
    }
}
