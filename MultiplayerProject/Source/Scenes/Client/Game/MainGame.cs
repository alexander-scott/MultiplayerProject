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

        private int _packetNumber = -1;
        private Queue<PlayerUpdatePacket> _updatePackets;

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

            _updatePackets = new Queue<PlayerUpdatePacket>();

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
                if (player.Value != _localPlayer) // Do not update the local player as we are already doing that in the input process
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

            // Process and fetch input from local player
            KeyboardMovementInput condensedInput = ProcessInputForLocalPlayer(gameTime, inputInfo);

            // Build an update packet from the input and player values
            PlayerUpdatePacket packet = _localPlayer.BuildUpdatePacket();
            packet.TotalGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            packet.Input = condensedInput;
            packet.SequenceNumber = _packetNumber++;

            // Add it to the queue
            _updatePackets.Enqueue(packet);

            if (sendPacketThisFrame)
            {
                // Send the packet to the server
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

            if (Application.APPLY_CLIENT_SIDE_PREDICTION)
            {
                _localPlayer.SetObjectState(input, (float)gameTime.ElapsedGameTime.TotalSeconds);
                _localPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            _localPlayer.Update(gameTime);

            return input;
        }

        private void ClientMessenger_OnRecievedRemotePlayerUpdate(PlayerUpdatePacket serverUpdate)
        {
            if (Application.APPLY_CLIENT_SIDE_RECONCILLIATION &&
                serverUpdate.PlayerID == _localPlayer.NetworkID && serverUpdate.SequenceNumber >= 0
                && _updatePackets.Count > 0)
            {
                PlayerUpdatePacket localUpdate = GetUpdateAtSequenceNumber(serverUpdate.SequenceNumber);

                if (localUpdate.XPosition != serverUpdate.XPosition
                    || localUpdate.YPosition != serverUpdate.YPosition
                    || localUpdate.Rotation != serverUpdate.Rotation
                    || localUpdate.Speed != serverUpdate.Speed)
                {
                    // Create a new queue with 'serverUpdate' as the first update
                    var newQueue = new Queue<PlayerUpdatePacket>();
                    var updateList = new List<PlayerUpdatePacket>();

                    PlayerUpdatePacket removedPacket = _updatePackets.Dequeue(); // Remove the first one which we are replacing with the serverUpdate

                    serverUpdate.TotalGameTime = removedPacket.TotalGameTime;
                    newQueue.Enqueue(serverUpdate);
                    updateList.Add(serverUpdate);

                    while (_updatePackets.Count > 0)
                    {
                        PlayerUpdatePacket packet = _updatePackets.Dequeue();
                        newQueue.Enqueue(packet);
                        updateList.Add(packet);
                    }

                    _updatePackets = newQueue; // Set the new queue

                    if (updateList.Count == 0)
                        return;

                    _localPlayer.SetObjectState(updateList[0]);
                    _localPlayer.Update(updateList[0].TotalGameTime);

                    if (updateList.Count == 1)
                        return;

                    // Now we must perform the previous inputs again
                    for (int i = 1; i < updateList.Count; i++)
                    {
                        _localPlayer.SetObjectState(updateList[i].Input, updateList[i].TotalGameTime);
                        _localPlayer.Update(updateList[i].TotalGameTime);
                    }
                }
            }
            else
            {
                Player remotePlayer = _players[serverUpdate.PlayerID];
                // APPLY INTERPOLATION HERE
                remotePlayer.SetObjectState(serverUpdate);
            }
        }

        private PlayerUpdatePacket GetUpdateAtSequenceNumber(int sequenceNumber)
        {
            PlayerUpdatePacket localUpdate;

            while (true)
            {
                localUpdate = _updatePackets.Peek();
                if (localUpdate.SequenceNumber != sequenceNumber)
                {
                    _updatePackets.Dequeue();
                }
                else
                {
                    break;
                }
            }

            return localUpdate;
        }
    }
}
