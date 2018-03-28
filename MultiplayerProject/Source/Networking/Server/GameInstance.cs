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
        private int framesSinceLastSend;

        public event EmptyDelegate OnReturnToGameRoom;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        private Dictionary<string, PlayerUpdatePacket> _playerUpdates;
        private Dictionary<string, LaserManager> _playerLasers;
        private Dictionary<string, Player> _players;

        private CollisionManager _collisionManager;

        public GameInstance(List<ServerConnection> clients)
        {
            ComponentClients = clients;

            _playerUpdates = new Dictionary<string, PlayerUpdatePacket>();
            _playerLasers = new Dictionary<string, LaserManager>();
            _players = new Dictionary<string, Player>();

            _collisionManager = new CollisionManager();

            var playerColours = GenerateRandomColours(clients.Count);

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].AddServerComponent(this);
                ComponentClients[i].SendPacketToClient(new GameInstanceInformation(ComponentClients.Count, ComponentClients, playerColours, ComponentClients[i].ID), MessageType.GI_ServerSend_LoadNewGame);
                _playerUpdates[ComponentClients[i].ID] = null;
                _playerLasers[ComponentClients[i].ID] = new LaserManager();

                Player player = new Player();
                 _players[ComponentClients[i].ID] = player;             
            }
        }

        public void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            switch (messageType)
            {
                case MessageType.GI_ClientSend_PlayerUpdatePacket:
                    {
                        var packet = packetBytes.DeserializeFromBytes<PlayerUpdatePacket>();
                        packet.PlayerID = client.ID;
                        _playerUpdates[client.ID] = packet;
                        break;
                    }

                case MessageType.GI_ClientSend_PlayerFiredPacket:
                    {
                        var packet = packetBytes.DeserializeFromBytes<PlayerFiredPacket>();
                        packet.PlayerID = client.ID;

                        var timeDifference = (packet.SendDate - DateTime.UtcNow).TotalSeconds;

                        var laser = _playerLasers[client.ID].FireLaserServer(packet.TotalGameTime, (float)timeDifference, new Vector2(packet.XPosition, packet.YPosition), packet.Rotation, packet.LaserID, packet.PlayerID);
                        if (laser != null)
                        {
                            for (int i = 0; i < ComponentClients.Count; i++)
                            {
                                ComponentClients[i].SendPacketToClient(packet, MessageType.GI_ServerSend_RemotePlayerFiredPacket);
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

            //_collisionManager.CheckCollision(_enemyManager.Enemies, _laserManager.Lasers, _explosionManager);

            if (sendPacketThisFrame)
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
    }
}