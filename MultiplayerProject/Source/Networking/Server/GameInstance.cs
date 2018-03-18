using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public class GameInstance : IMessageable
    {
        public event EmptyDelegate OnReturnToGameRoom;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        public GameInstance(List<ServerConnection> clients)
        {
            ComponentClients = clients;

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].AddServerComponent(this);
                ComponentClients[i].SendPacketToClient(new RoomInformation(null, null, ComponentClients, ComponentClients.Count, true), MessageType.GI_ServerSend_LoadNewGame);
            }
        }

        public void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            switch (messageType)
            {

            }
        }

        public void RemoveClient(ServerConnection client)
        {
            throw new NotImplementedException();
        }
    }
}
