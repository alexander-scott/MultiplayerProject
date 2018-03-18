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
        private Player _localPlayer;
        private Client _client;

        private EnemyManager _enemyManager;
        private LaserManager _laserManager;
        private CollisionManager _collisionManager;
        private ExplosionManager _explosionManager;
        private BackgroundManager _backgroundManager;

        private int framesSinceLastSend;
        private int framesBetweenPackets = 6;

        public MainGame(int width, int height, int playerCount, string[] playerIDs, string localPlayerID, Client client)
        {
            Width = width;
            Height = height;

            _players = new List<Player>();
            _client = client; 

            for (int i = 0; i < playerCount; i++)
            {
                Player player = new Player(width, height);
                player.NetworkID = playerIDs[i];

                if (playerIDs[i] == localPlayerID)
                {
                    _localPlayer = player;
                    player.IsLocal = true;
                } 
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
            // Is it time to send outgoing network packets?
            bool sendPacketThisFrame = false;

            framesSinceLastSend++;

            if (framesSinceLastSend >= framesBetweenPackets)
            {
                sendPacketThisFrame = true;
                framesSinceLastSend = 0;
            }

            KeyboardMovementInput condensedInput = ProcessInputForLocalPlayer(gameTime, inputInfo);

            // SEND UPDATE PACKET TO SERVER
            if (sendPacketThisFrame)
            {
                PlayerUpdatePacket packet = _localPlayer.BuildUpdatePacket();
                packet.TotalGameTime = (float)gameTime.TotalGameTime.TotalSeconds;
                packet.Input = condensedInput;

                _client.SendMessageToServer(packet, MessageType.GI_ClientSend_PlayerUpdatePacket);
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
        
        private KeyboardMovementInput ProcessInputForLocalPlayer(GameTime gameTime, InputInformation inputInfo)
        {
            KeyboardMovementInput input = new KeyboardMovementInput();

            // Keyboard/Dpad controls
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Left) || inputInfo.CurrentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                _localPlayer.RotateLeft(gameTime);
                input.LeftPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Right) || inputInfo.CurrentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                _localPlayer.RotateRight(gameTime);
                input.RightPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Up) || inputInfo.CurrentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                _localPlayer.MoveForward(gameTime);
                input.UpPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Down) || inputInfo.CurrentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                _localPlayer.MoveBackward(gameTime);
                input.DownPressed = true;
            }

            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Space) || inputInfo.CurrentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                _laserManager.FireLaser(gameTime, _localPlayer.Position, _localPlayer.Rotation);
                input.FirePressed = true;
            }

            return input;
        }
    }
}
