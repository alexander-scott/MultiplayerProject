using System;
using System.Collections.Generic;
using System.IO;

using MultiplayerProject.Source;

namespace MultiplayerProject.Source
{
    public class WaitingRoom : IMessageable
    {
        public const int MAX_PEOPLE_PER_LOBBY = 6;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        private Dictionary<string,Lobby> _activeLobbys;
        private int _maxLobbies;

        public WaitingRoom(int maxLobbies)
        {
            ComponentType = MessageableComponent.WaitingRoom;
            ComponentClients = new List<ServerConnection>();
            _maxLobbies = maxLobbies;
            _activeLobbys = new Dictionary<string, Lobby>();
        }

        public void AddClientToWaitingRoom(ServerConnection connection)
        {
            ComponentClients.Add(connection);

            connection.AddServerComponent(this);
            connection.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_FullInfo);
        }

        public void CreateNewLobby(string lobbyName)
        {
            if (_activeLobbys.Count < _maxLobbies)
            {
                Lobby newLobby = new Lobby(MAX_PEOPLE_PER_LOBBY, lobbyName);
                _activeLobbys.Add(newLobby.ID, newLobby);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public WaitingRoomInformation GetWaitingRoomInformation()
        {
            WaitingRoomInformation waitingRoomInfo = new WaitingRoomInformation
            {
                LobbyCount = _activeLobbys.Count,
                Lobbies = new LobbyInformation[_activeLobbys.Count]
            };

            int count = 0;
            foreach (KeyValuePair<string, Lobby> entry in _activeLobbys)
            {
                waitingRoomInfo.Lobbies[count] = entry.Value.GetLobbyInformation();
                count++;
            }

            return waitingRoomInfo;
        }

        public void RecieveClientMessage(ServerConnection client, MessageType type, byte[] buffer)
        {
            switch (type)
            {
                case MessageType.WR_ClientRequest_CreateLobby:
                {
                    if (_activeLobbys.Count < Server.MAX_LOBBIES)
                    {
                        CreateNewLobby("TEST NEW LOBBY " + _activeLobbys.Count);
                        foreach (var connectedClient in ComponentClients)
                        {
                            connectedClient.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_FullInfo);
                        }
                    }
                    else
                    {
                        client.SendPacketToClient(new BasePacket(), MessageType.WR_ServerResponse_FailCreateLobby);
                    }
                    break;
                }
                    
                case MessageType.WR_ClientRequest_JoinLobby:
                {
                    StringPacket joinPacket = buffer.DeserializeFromBytes<StringPacket>();
                    Lobby joinedLobby = _activeLobbys[joinPacket.String];
                    if (joinedLobby.ComponentClients.Count < MAX_PEOPLE_PER_LOBBY)
                    {
                        client.SendPacketToClient(new StringPacket(joinPacket.String), MessageType.WR_ServerResponse_SuccessJoinLobby);
                        joinedLobby.AddClientToLobby(client);
                        foreach (var connectedClient in ComponentClients)
                        {
                            connectedClient.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_FullInfo);
                        }
                    }
                    else
                    {
                        client.SendPacketToClient(new BasePacket(), MessageType.WR_ServerResponse_FailJoinLobby);
                    }
                    break;
                }
                    
                case MessageType.WR_ClientRequest_LeaveLobby:
                {
                    StringPacket leavePacket = buffer.DeserializeFromBytes<StringPacket>();
                    Lobby leaveLobby = _activeLobbys[leavePacket.String];
                    client.SendPacketToClient(new StringPacket(leavePacket.String), MessageType.WR_ServerResponse_SuccessLeaveLobby);
                    leaveLobby.RemoveClient(client);
                    foreach (var connectedClient in ComponentClients)
                    {
                        connectedClient.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_FullInfo);
                    }
                    break;
                }            
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
            client.RemoveServerComponent(this);
        }        
    }
}
