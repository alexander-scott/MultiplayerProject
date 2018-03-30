using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    public class ServerLeaderboard : IMessageable
    {
        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        public ServerLeaderboard(List<ServerConnection> clients)
        {
            ComponentClients = clients;

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].AddServerComponent(this);
            }
        }

        public void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            throw new NotImplementedException();
        }

        public void RemoveClient(ServerConnection client)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
