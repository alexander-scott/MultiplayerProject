using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    /*
     HOW SYNCING LASER FIRING IS GOING TO WORK:
     - On the client side, when fire is pressed, a local laser is fired immediately from the player ship. A network fire packet is sent to server and then sent to clients.
     - No laser collision checks take place on the client side.

     - On the server side, each 'Player' has a LaserManager instance. 
     - When the server recieves the fire packet from the client, it updates it by the deltaTime between sending and recieving the packet, to sync it perfectly to the client
     - All Player LaserManager's and their Laser's are updated every frame
     - On the server side there is also a CollisionManager instance, checking for collisions between any any laser and any player.
     - If there is a collision, send a player killed message back to all clients, triggering a death explosion on the client. Mark the player as dead on the server too.
         
         */
    public class GameInstance : IMessageable
    {
        public event BasePacketDelegate OnGameCompleted;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        private string _gameRoomID;

        private Dictionary<string, PlayerUpdatePacket> _playerUpdates;
        private Dictionary<string, LaserManager> _playerLasers;
        private Dictionary<string, int> _playerScores;
        private Dictionary<string, string> _playerNames;
        private Dictionary<string, Player> _players;
        private Dictionary<string, Color> _playerColours;

        private CollisionManager _collisionManager;

        private EnemyManager _enemyManager;
        private TimeSpan _enemySpawnTime;
        private TimeSpan _previousEnemySpawnTime;
        private int framesSinceLastSend;

        public GameInstance(List<ServerConnection> clients, string gameRoomID)
        {
            ComponentClients = clients;

            _gameRoomID = gameRoomID;

            _playerUpdates = new Dictionary<string, PlayerUpdatePacket>();
            _playerLasers = new Dictionary<string, LaserManager>();
            _playerScores = new Dictionary<string, int>();
            _playerNames = new Dictionary<string, string>();
            _playerColours = new Dictionary<string, Color>();
            _players = new Dictionary<string, Player>();

            _collisionManager = new CollisionManager();

            _enemyManager = new EnemyManager();
            _previousEnemySpawnTime = TimeSpan.Zero;
            _enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            var playerColours = GenerateRandomColours(clients.Count);

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].AddServerComponent(this);
                ComponentClients[i].SendPacketToClient(new GameInstanceInformation(ComponentClients.Count, ComponentClients, playerColours, ComponentClients[i].ID), MessageType.GI_ServerSend_LoadNewGame);

                _playerUpdates[ComponentClients[i].ID] = null;
                _playerLasers[ComponentClients[i].ID] = new LaserManager();
                _playerScores[ComponentClients[i].ID] = 0;
                _playerNames[ComponentClients[i].ID] = ComponentClients[i].Name;
                _playerColours[ComponentClients[i].ID] = playerColours[i];

                Player player = new Player();
                player.NetworkID = ComponentClients[i].ID;
                 _players[ComponentClients[i].ID] = player;             
            }
        }

        public void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            switch (messageType)
            {
                case MessageType.GI_ClientSend_PlayerUpdate:
                    {
                        var packet = packetBytes.DeserializeFromBytes<PlayerUpdatePacket>();
                        packet.PlayerID = client.ID;
                        _playerUpdates[client.ID] = packet;
                        break;
                    }

                case MessageType.GI_ClientSend_PlayerFired:
                    {
                        var packet = packetBytes.DeserializeFromBytes<PlayerFiredPacket>();
                        packet.PlayerID = client.ID;

                        var timeDifference = (packet.SendDate - DateTime.UtcNow).TotalSeconds;

                        var laser = _playerLasers[client.ID].FireLaserServer(packet.TotalGameTime, (float)timeDifference, new Vector2(packet.XPosition, packet.YPosition), packet.Rotation, packet.LaserID, packet.PlayerID);
                        if (laser != null)
                        {
                            for (int i = 0; i < ComponentClients.Count; i++)
                            {
                                ComponentClients[i].SendPacketToClient(packet, MessageType.GI_ServerSend_RemotePlayerFired);
                            }
                        }
                        break;
                    }
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            bool sendPacketThisFrame = false;

            framesSinceLastSend++;

            if (framesSinceLastSend >= Application.SERVER_UPDATE_RATE)
            {
                sendPacketThisFrame = true;
                framesSinceLastSend = 0;
            }

            ApplyPlayerInput(gameTime);

            UpdateEnemies(gameTime);
            
            CheckCollisions();

            if (sendPacketThisFrame)
            {
                SendPlayerStatesToClients();
            }
        }

        private void ApplyPlayerInput(GameTime gameTime)
        {
            // Apply the inputs recieved from the clients to the simulation running on the server
            foreach (KeyValuePair<string, Player> player in _players)
            {
                if (_playerUpdates[player.Key] != null)
                {
                    player.Value.ApplyInputToPlayer(_playerUpdates[player.Key].Input, (float)gameTime.ElapsedGameTime.TotalSeconds);
                    player.Value.LastSequenceNumberProcessed = _playerUpdates[player.Key].SequenceNumber;
                    player.Value.LastKeyboardMovementInput = _playerUpdates[player.Key].Input;

                    player.Value.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }

                if (_playerLasers[player.Key] != null)
                {
                    _playerLasers[player.Key].Update(gameTime);
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if (gameTime.TotalGameTime - _previousEnemySpawnTime > _enemySpawnTime)
            {
                _previousEnemySpawnTime = gameTime.TotalGameTime;

                var enemy = _enemyManager.AddEnemy();

                for (int i = 0; i < ComponentClients.Count; i++) // Send the enemy spawn to all clients
                {
                    EnemySpawnedPacket packet = new EnemySpawnedPacket(enemy.Position.X, enemy.Position.Y, enemy.EnemyID);
                    packet.TotalGameTime = (float)gameTime.TotalGameTime.TotalSeconds;

                    ComponentClients[i].SendPacketToClient(packet, MessageType.GI_ServerSend_EnemySpawn);
                }
            }

            _enemyManager.Update(gameTime);
        }

        private void CheckCollisions()
        {
            var collisions = _collisionManager.CheckCollision(_players.Values.ToList(), _enemyManager.Enemies, GetActiveLasers());

            if (collisions.Count > 0)
            {
                for (int iCollision = 0; iCollision < collisions.Count; iCollision++)
                {
                    _playerLasers[collisions[iCollision].AttackingPlayerID].DeactivateLaser(collisions[iCollision].LaserID); // Deactivate collided laser

                    if (collisions[iCollision].CollisionType == CollisionManager.CollisionType.LaserToEnemy)
                    {                      
                        _enemyManager.DeactivateEnemy(collisions[iCollision].DefeatedEnemyID); // Deactivate collided enemy

                        // INCREMENT PLAYER SCORE HERE
                        _playerScores[collisions[iCollision].AttackingPlayerID]++;

                        // Create packet to send to clients
                        EnemyDefeatedPacket packet = new EnemyDefeatedPacket(collisions[iCollision].LaserID, collisions[iCollision].DefeatedEnemyID, collisions[iCollision].AttackingPlayerID, _playerScores[collisions[iCollision].AttackingPlayerID]);
                        for (int iClient = 0; iClient < ComponentClients.Count; iClient++)
                        {
                            ComponentClients[iClient].SendPacketToClient(packet, MessageType.GI_ServerSend_EnemyDefeated);
                        }
                    }
                    else
                    {
                        // DECCREMENT PLAYER SCORE HERE
                        if (_playerScores[collisions[iCollision].DefeatedPlayerID] > 0)
                            _playerScores[collisions[iCollision].DefeatedPlayerID]--;

                        // Create packet to send to clients
                        // TODO: In future move player to a random spot on the map and send that data with this packet
                        PlayerDefeatedPacket packet = new PlayerDefeatedPacket(collisions[iCollision].LaserID, collisions[iCollision].DefeatedPlayerID, _playerScores[collisions[iCollision].DefeatedPlayerID]);
                        for (int iClient = 0; iClient < ComponentClients.Count; iClient++)
                        {
                            ComponentClients[iClient].SendPacketToClient(packet, MessageType.GI_ServerSend_PlayerDefeated);
                        }
                    }
                }

                CheckGameOver();
            }
        }

        private void SendPlayerStatesToClients()
        {
            // Send a copy of the simulation on the server to all clients
            for (int i = 0; i < ComponentClients.Count; i++)
            {
                foreach (KeyValuePair<string, Player> player in _players)
                {
                    PlayerUpdatePacket updatePacket;
                    updatePacket = player.Value.BuildUpdatePacket(); // Here we are using the servers values which makes it authorative over the clients
                    updatePacket.PlayerID = player.Key;
                    updatePacket.SequenceNumber = player.Value.LastSequenceNumberProcessed;
                    updatePacket.Input = player.Value.LastKeyboardMovementInput;

                    ComponentClients[i].SendPacketToClient(updatePacket, MessageType.GI_ServerSend_UpdateRemotePlayer);
                }
            }
        }

        private void CheckGameOver()
        {
            foreach (KeyValuePair<string, int> player in _playerScores)
            {
                if (player.Value >= Application.SCORE_TO_WIN)
                {
                    Console.WriteLine("GAME OVER");

                    int playerCount = ComponentClients.Count;
                    int[] playerScores = new int[playerCount];
                    string[] playerNames = new string[playerCount];

                    int index = 0;
                    foreach (KeyValuePair<string, int> playerScore in _playerScores)
                    {
                        playerScores[index] = playerScore.Value;
                        playerNames[index] = _playerNames[playerScore.Key];
                        index++;
                    }

                    PlayerColour[] playerColours = new PlayerColour[_playerColours.Count];
                    index = 0;
                    foreach (KeyValuePair<string, Color> playerColour in _playerColours)
                    {
                        playerColours[index] = new PlayerColour(playerColour.Value.R, playerColour.Value.G, playerColour.Value.B);
                        index++;
                    }

                    LeaderboardPacket packet = new LeaderboardPacket(playerCount, playerNames, playerScores, playerColours);
                    for (int iClient = 0; iClient < ComponentClients.Count; iClient++)
                    {
                        ComponentClients[iClient].SendPacketToClient(packet, MessageType.GI_ServerSend_GameOver);
                    }

                    OnGameCompleted(packet);

                    return;
                }
            }
        }

        private List<Color> GenerateRandomColours(int playerCount)
        {
            var returnList = new List<Color>();
            for (int i = 0; i < playerCount && i < WaitingRoom.MAX_PEOPLE_PER_ROOM; i++)
            {
                switch(i)
                {
                    case 0:
                        returnList.Add(Color.White);
                        break;
                    case 1:
                        returnList.Add(Color.Red);
                        break;
                    case 2:
                        returnList.Add(Color.Blue);
                        break;
                    case 3:
                        returnList.Add(Color.Green);
                        break;
                    case 4:
                        returnList.Add(Color.Aqua);
                        break;
                    case 5:
                        returnList.Add(Color.Pink);
                        break;
                }
            }
            return returnList;
        }

        private List<Laser> GetActiveLasers()
        {
            List<Laser> lasers = new List<Laser>();

            foreach (KeyValuePair<string, LaserManager> laserManager in _playerLasers)
            { 
                lasers.AddRange(laserManager.Value.Lasers);
            }

            return lasers;
        }
    }
}