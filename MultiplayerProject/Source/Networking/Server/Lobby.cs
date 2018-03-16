using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    class Lobby
    {
        public string LobbyName;
        public string ID;

        private List<ServerConnection> _connectedClients;

        private int _maxConnections;
        private int _connections;

        public Lobby(int maxConnections, string name)
        {
            _maxConnections = maxConnections;
            LobbyName = name;

            ID = Guid.NewGuid().ToString();
            _connectedClients = new List<ServerConnection>();
        }

        public void AddClientToLobby(ServerConnection client)
        {
            _connectedClients.Add(client);
            _connections++;
        }

        public void RemoveClientFromLobby(ServerConnection client)
        {
            _connectedClients.Remove(client);
            _connections--;
        }

        public LobbyInformation GetLobbyInformation()
        {
            return new LobbyInformation(LobbyName, ID, _connectedClients);
        }
    }
}
