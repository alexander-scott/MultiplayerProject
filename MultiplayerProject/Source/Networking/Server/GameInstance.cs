using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    /*
     HOW THIS WILL BE ACHIEVED STEP 1:
     - Create a list of player objects in the main game. One of these player is a local player. 
        Rest are remote players with ID that aren't affected by local inputs.
     - Local inputs move local player instantly.
     Every 6 frames (adjustable interval) local player will send an update packet to this server. This packet will contain:
     - gameTime.TotalGameTime.TotalSeconds
     - player.Position
     - player.Velocity
     - player.Rotation
     - currentKeyboardInput

    HOW THIS WILL BE ACHIEVED STEP 2:
    - At a select interval the server will send the update packet of remote players to the other players
    - When the local client recieves the update packet it will set the respective players position/rotation to
        contents of the packet.

    HOW THIS WILL BE ACHIEVED STEP 3:
    - The server will apply prediction and smoothing to the player packets it recieves in an effort to reduce
        latency and lag
    - Alternatively the clients could do the prediction and smoothing and the server could simply just act as a 
        messaging service. However this would make the server unauthoratitive.



        SERVER RECONCILLIATION:
        Client or MainGame will keep a list of update packets it has sent to the server. Each update packet has an ID
        or sequence number. When it recieves an update back from the server, it checks the last sequence number the
        server was able to process. Then the client will edit the list, removing all entries up to last sequence number
        that the server processed. The client will then iterate over the list of update packets it has saved up, performing
        the list of inputs again, except this time it starts from the processed packet the server sent back.
         
         */
    public class GameInstance : IMessageable
    {
        private int framesSinceLastSend;

        public event EmptyDelegate OnReturnToGameRoom;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        private Dictionary<string, PlayerUpdatePacket> _playerUpdates;
        private Dictionary<string, Player> _players;

        public GameInstance(List<ServerConnection> clients)
        {
            ComponentClients = clients;
            _playerUpdates = new Dictionary<string, PlayerUpdatePacket>();
            _players = new Dictionary<string, Player>();

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].AddServerComponent(this);
                ComponentClients[i].SendPacketToClient(new GameInstanceInformation(ComponentClients.Count, ComponentClients, ComponentClients[i].ID), MessageType.GI_ServerSend_LoadNewGame);
                _playerUpdates[ComponentClients[i].ID] = null;

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
                    player.Value.SetObjectState(_playerUpdates[player.Key].Input, (float)gameTime.ElapsedGameTime.TotalSeconds);
                    player.Value.LastSequenceNumberProcessed = _playerUpdates[player.Key].SequenceNumber;

                    player.Value.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }

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

                        ComponentClients[i].SendPacketToClient(updatePacket, MessageType.GI_ServerSend_UpdateRemotePlayer);
                    }
                }
            }
        }
    }
}