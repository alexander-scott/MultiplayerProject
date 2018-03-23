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
         
         */
    public class GameInstance : IMessageable
    {
        private int framesSinceLastSend;
        private int framesBetweenPackets = 20;

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

                _players[ComponentClients[i].ID] = new Player();
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

            if (framesSinceLastSend >= framesBetweenPackets)
            {
                sendPacketThisFrame = true;
                framesSinceLastSend = 0;
            }

            if (sendPacketThisFrame)
            {
                for (int i = 0; i < ComponentClients.Count; i++)
                {
                    foreach (PlayerUpdatePacket playerUpdate in _playerUpdates.Values.ToList())
                    {
                        if (playerUpdate != null)
                        {
                            ComponentClients[i].SendPacketToClient(playerUpdate, MessageType.GI_ServerSend_UpdateRemotePlayer);
                        } 
                    }
                }
            }
        }
    }
}
