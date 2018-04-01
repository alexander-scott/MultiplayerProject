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
        public static event ServerConnectionDelegate OnClientLeaveGameRoom;
        public static event EmptyDelegate OnStartRematch;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }
        private Dictionary<ServerConnection, bool> _clientReadyStatus;
        private string _gameRoomID;

        public ServerLeaderboard(List<ServerConnection> clients, string gameroomID)
        {
            ComponentClients = clients;
            _gameRoomID = gameroomID;
            _clientReadyStatus = new Dictionary<ServerConnection, bool>();

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].AddServerComponent(this);
                _clientReadyStatus[ComponentClients[i]] = false;
            }
        }

        public void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            switch (messageType)
            {
                case MessageType.LB_ClientSend_RematchReady:
                    {
                        _clientReadyStatus[client] = true;
                        break;
                    }

                case MessageType.LB_ClientSend_RematchUnready:
                    {
                        _clientReadyStatus[client] = false;
                        break;
                    }

                case MessageType.LB_ClientSend_ReturnToWaitingRoom:
                    {
                        client.RemoveServerComponent(this);
                        RemoveClient(client);
                        OnClientLeaveGameRoom(client, _gameRoomID);
                        break;
                    }
            }

            UpdateLobbyState();
            CheckRestartGame();
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
        }

        public void Update(GameTime gameTime)
        {
            //throw new NotImplementedException();
        }

        private void UpdateLobbyState()
        {
            int readyCount = GetReadyCount();

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                LeaderboardUpdatePacket packet = new LeaderboardUpdatePacket(ComponentClients.Count, readyCount, _clientReadyStatus[ComponentClients[i]]);
                ComponentClients[i].SendPacketToClient(packet, MessageType.LB_ServerSend_UpdateLeaderboard);
            }               
        }

        private int GetReadyCount()
        {
            int numberOfClients = ComponentClients.Count;

            int readyCount = 0;
            for (int i = 0; i < numberOfClients; i++)
            {
                if (_clientReadyStatus[ComponentClients[i]])
                {
                    readyCount++;
                }
            }

            return readyCount;
        }

        private void CheckRestartGame()
        {
            if (GetReadyCount() == ComponentClients.Count)
            {
                for (int i = 0; i < ComponentClients.Count; i++)
                {
                    ComponentClients[i].RemoveServerComponent(this);
                }
                OnStartRematch();
            }
        }
    }
}
