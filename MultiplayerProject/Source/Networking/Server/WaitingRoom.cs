using System;
using System.Collections.Generic;
using System.IO;

using MultiplayerProject.Source;

namespace MultiplayerProject.Source
{
    public class WaitingRoom : IMessageable
    {
        public const int MAX_PEOPLE_PER_ROOM = 6;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        private Dictionary<string,GameRoom> _activeRooms;
        private int _maxRooms;

        public WaitingRoom(int maxLobbies)
        {
            ComponentType = MessageableComponent.WaitingRoom;
            ComponentClients = new List<ServerConnection>();
            _maxRooms = maxLobbies;
            _activeRooms = new Dictionary<string, GameRoom>();
        }

        public void AddClientToWaitingRoom(ServerConnection connection)
        {
            ComponentClients.Add(connection);

            connection.AddServerComponent(this);
            connection.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_FullInfo);
        }

        public void CreateNewRoom(string roomName)
        {
            if (_activeRooms.Count < _maxRooms)
            {
                GameRoom newRoom = new GameRoom(MAX_PEOPLE_PER_ROOM, roomName);
                _activeRooms.Add(newRoom.ID, newRoom);
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
                RoomCount = _activeRooms.Count,
                Rooms = new RoomInformation[_activeRooms.Count]
            };

            int count = 0;
            foreach (KeyValuePair<string, GameRoom> entry in _activeRooms)
            {
                waitingRoomInfo.Rooms[count] = entry.Value.GetRoomInformation();
                count++;
            }

            return waitingRoomInfo;
        }

        public void RecieveClientMessage(ServerConnection client, MessageType type, byte[] buffer)
        {
            switch (type)
            {
                case MessageType.WR_ClientRequest_CreateRoom:
                {
                    if (_activeRooms.Count < Server.MAX_ROOMS)
                    {
                        CreateNewRoom("TEST NEW ROOM " + _activeRooms.Count);
                        foreach (var connectedClient in ComponentClients)
                        {
                            connectedClient.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_FullInfo);
                        }
                    }
                    else
                    {
                        client.SendPacketToClient(new BasePacket(), MessageType.WR_ServerResponse_FailCreateRoom);
                    }
                    break;
                }
                    
                case MessageType.WR_ClientRequest_JoinRoom:
                {
                    StringPacket joinPacket = buffer.DeserializeFromBytes<StringPacket>();
                    GameRoom joinedRoom = _activeRooms[joinPacket.String];
                    if (joinedRoom.ComponentClients.Count < MAX_PEOPLE_PER_ROOM)
                    {
                        client.SendPacketToClient(new StringPacket(joinPacket.String), MessageType.WR_ServerResponse_SuccessJoinRoom);
                        joinedRoom.AddClientToRoom(client);
                        foreach (var connectedClient in ComponentClients)
                        {
                            connectedClient.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_FullInfo);
                        }
                    }
                    else
                    {
                        client.SendPacketToClient(new BasePacket(), MessageType.WR_ServerResponse_FailJoinRoom);
                    }
                    break;
                }
                    
                case MessageType.WR_ClientRequest_LeaveRoom:
                {
                    StringPacket leavePacket = buffer.DeserializeFromBytes<StringPacket>();
                    GameRoom room = _activeRooms[leavePacket.String];
                    client.SendPacketToClient(new StringPacket(leavePacket.String), MessageType.WR_ServerResponse_SuccessLeaveRoom);
                    room.RemoveClient(client);
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
