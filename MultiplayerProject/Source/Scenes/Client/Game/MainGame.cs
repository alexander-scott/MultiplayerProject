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
        private List<RemotePlayer> _remotePlayers;
        private LocalPlayer _localPlayer;
        private Dictionary<string, PlayerColour> _playerColours;

        private Client _client;

        private EnemyManager _enemyManager;
        private LaserManager _laserManager;
        private ExplosionManager _explosionManager;
        private BackgroundManager _backgroundManager;

        private int framesSinceLastSend;

        private int _packetNumber = -1;
        private Queue<PlayerUpdatePacket> _updatePackets;

        public MainGame(int playerCount, string[] playerIDs, PlayerColour[] playerColours, string localClientID, Client client)
        {
            _players = new Dictionary<string, Player>();
            _playerColours = new Dictionary<string, PlayerColour>();
            _remotePlayers = new List<RemotePlayer>();

            _client = client; 

            for (int i = 0; i < playerCount; i++)
            {
                Player player; 
                if (playerIDs[i] == localClientID)
                {
                    var locPlay = new LocalPlayer();
                    _localPlayer = locPlay;
                    player = locPlay;
                } 
                else
                {
                    var remPlay = new RemotePlayer();
                    _remotePlayers.Add(remPlay);
                    player = remPlay;
                }

                player.NetworkID = playerIDs[i];
                _playerColours.Add(playerIDs[i], playerColours[i]);

                _players.Add(player.NetworkID, player);
            }

            _updatePackets = new Queue<PlayerUpdatePacket>();

            _enemyManager = new EnemyManager();
            _laserManager = new LaserManager();
            _backgroundManager = new BackgroundManager();

            _explosionManager = new ExplosionManager();

            ClientMessenger.OnRecievedPlayerUpdatePacket += OnRecievedPlayerUpdatePacket;
            ClientMessenger.OnRecievedPlayerFiredPacket += ClientMessenger_OnRecievedPlayerFiredPacket;
            ClientMessenger.OnEnemySpawnedPacket += ClientMessenger_OnEnemySpawnedPacket;
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            foreach (KeyValuePair<string, Player> player in _players)
            {
                player.Value.Initialize(content, _playerColours[player.Key]);
            }

            _enemyManager.Initalise(content);
            _laserManager.Initalise(content);
            _explosionManager.Initalise(content);
            _backgroundManager.Initalise(content);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < _remotePlayers.Count; i++)
            {
                _remotePlayers[i].UpdateRemote(Application.CLIENT_UPDATE_RATE, (float)gameTime.ElapsedGameTime.TotalSeconds);
                _remotePlayers[i].UpdateAnimation(gameTime);
            }

            _backgroundManager.Update(gameTime);
            _enemyManager.Update(gameTime);
            _laserManager.Update(gameTime);
            _explosionManager.Update(gameTime);
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
            packet.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
                var laser = _laserManager.FireLocalLaserClient(gameTime, _localPlayer.Position, _localPlayer.Rotation, _playerColours[_localPlayer.NetworkID]);
                if (laser != null)
                {
                    input.FirePressed = true;
                    var dataPacket = _localPlayer.BuildUpdatePacket();
                    PlayerFiredPacket packet = new PlayerFiredPacket(dataPacket.XPosition, dataPacket.YPosition, dataPacket.Speed, dataPacket.Rotation);
                    packet.TotalGameTime = (float)gameTime.TotalGameTime.TotalSeconds; // TOTAL GAME TIME NOT ELAPSED TIME!
                    packet.LaserID = laser.LaserID;
                    
                    // Send the packet to the server
                    _client.SendMessageToServer(packet, MessageType.GI_ClientSend_PlayerFiredPacket);
                }
            }

            if (Application.APPLY_CLIENT_SIDE_PREDICTION)
            {
                _localPlayer.ApplyInputToPlayer(input, (float)gameTime.ElapsedGameTime.TotalSeconds);
                _localPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            _localPlayer.UpdateAnimation(gameTime);

            return input;
        }

        private void OnRecievedPlayerUpdatePacket(PlayerUpdatePacket serverUpdate)
        {
            if (Application.APPLY_SERVER_RECONCILLIATION &&
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

                    serverUpdate.DeltaTime = removedPacket.DeltaTime;
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

                    _localPlayer.SetPlayerState(updateList[0]);
                    _localPlayer.Update(updateList[0].DeltaTime);

                    if (updateList.Count == 1)
                        return;

                    // Now we must perform the previous inputs again
                    for (int i = 1; i < updateList.Count; i++)
                    {
                        _localPlayer.ApplyInputToPlayer(updateList[i].Input, updateList[i].DeltaTime);
                        _localPlayer.Update(updateList[i].DeltaTime);
                    }
                }
            }
            else
            {
                RemotePlayer remotePlayer = _players[serverUpdate.PlayerID] as RemotePlayer;
                remotePlayer.SetUpdatePacket(serverUpdate);
            }
        }

        private void ClientMessenger_OnRecievedPlayerFiredPacket(PlayerFiredPacket playerUpdate)
        {
            if (playerUpdate.PlayerID != _localPlayer.NetworkID) // Local laser has already been shot so don't shoot it again
            {
                _laserManager.FireRemoteLaserClient(new Vector2(playerUpdate.XPosition, playerUpdate.YPosition), playerUpdate.Rotation, playerUpdate.PlayerID, playerUpdate.SendDate, playerUpdate.LaserID, _playerColours[playerUpdate.PlayerID]);
            }
        }

        private void ClientMessenger_OnEnemySpawnedPacket(EnemySpawnedPacket enemySpawn)
        {
            _enemyManager.AddEnemy(new Vector2(enemySpawn.XPosition, enemySpawn.YPosition), enemySpawn.EnemyID);
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