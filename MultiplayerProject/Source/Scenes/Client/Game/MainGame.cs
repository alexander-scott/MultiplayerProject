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
        private Dictionary<string,Player> _players;
        private Player _localPlayer;
        private Client _client;

        private EnemyManager _enemyManager;
        private LaserManager _laserManager;
        private CollisionManager _collisionManager;
        private ExplosionManager _explosionManager;
        private BackgroundManager _backgroundManager;

        private int framesSinceLastSend;

        private int _packetNumber = 0;
        private List<PlayerUpdatePacket> _updatePacketsSent;

        public MainGame(int playerCount, string[] playerIDs, string localPlayerID, Client client)
        {
            _players = new Dictionary<string, Player>();
            _client = client; 

            for (int i = 0; i < playerCount; i++)
            {
                Player player = new Player();
                player.NetworkID = playerIDs[i];

                if (player.NetworkID == localPlayerID)
                {
                    _localPlayer = player;
                    player.IsLocal = true;
                } 
                else
                    player.IsLocal = false;

                _players.Add(player.NetworkID, player);
            }

            _updatePacketsSent = new List<PlayerUpdatePacket>();

            _enemyManager = new EnemyManager();
            _laserManager = new LaserManager();
            _backgroundManager = new BackgroundManager();

            _explosionManager = new ExplosionManager();
            _collisionManager = new CollisionManager();

            ClientMessenger.OnRecievedRemotePlayerUpdate += ClientMessenger_OnRecievedRemotePlayerUpdate;
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            foreach (KeyValuePair<string, Player> player in _players)
            {
                player.Value.Initialize(content);
            }

            _enemyManager.Initalise(content);
            _laserManager.Initalise(content);
            _explosionManager.Initalise(content);
            _backgroundManager.Initalise(content);
        }

        public void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<string, Player> player in _players)
            {
                player.Value.Update(gameTime);
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

            if (framesSinceLastSend >= Application.CLIENT_UPDATE_RATE)
            {
                sendPacketThisFrame = true;
                framesSinceLastSend = 0;
            }

            KeyboardMovementInput condensedInput = ProcessInputForLocalPlayer(gameTime, inputInfo);

            PlayerUpdatePacket packet = _localPlayer.BuildUpdatePacket();
            packet.TotalGameTime = (float)gameTime.TotalGameTime.TotalSeconds;
            packet.Input = condensedInput;
            packet.PacketNumber = _packetNumber++;

            _updatePacketsSent.Add(packet);

            // SEND UPDATE PACKET TO SERVER
            if (sendPacketThisFrame)
            {
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

            foreach (KeyValuePair<string, Player> player in _players)
            {
                player.Value.Draw(spriteBatch);
            }
        }    
        
        private KeyboardMovementInput ProcessInputForLocalPlayer(GameTime gameTime, InputInformation inputInfo)
        {
            KeyboardMovementInput input = new KeyboardMovementInput();

            // Keyboard/Dpad controls
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Left) || inputInfo.CurrentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                input.LeftPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Right) || inputInfo.CurrentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                input.RightPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Up) || inputInfo.CurrentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                input.UpPressed = true;
            }
            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Down) || inputInfo.CurrentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                input.DownPressed = true;
            }

            if (inputInfo.CurrentKeyboardState.IsKeyDown(Keys.Space) || inputInfo.CurrentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                _laserManager.FireLaser(gameTime, _localPlayer.Position, _localPlayer.Rotation);
                input.FirePressed = true;
            }

            if (Application.CLIENT_SIDE_PREDICTION)
            {
                _localPlayer.SetObjectStateLocal(input, gameTime);
            }

            return input;
        }

        private void ClientMessenger_OnRecievedRemotePlayerUpdate(PlayerUpdatePacket playerUpdate)
        {
            Player remotePlayer = _players[playerUpdate.PlayerID];
            remotePlayer.SetObjectState(playerUpdate);
        }
    }
}
