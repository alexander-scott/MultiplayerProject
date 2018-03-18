using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class GameInstance : IMessageable
    {
        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        public GameInstance(List<ServerConnection> clients)
        {
            ComponentClients = clients;
        }

        public void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            throw new NotImplementedException();
        }

        public void RemoveClient(ServerConnection client)
        {
            throw new NotImplementedException();
        }
    }
}
