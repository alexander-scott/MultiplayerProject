using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    public class WaitingRoom : IMessageable
    {
        public const int MAX_PEOPLE_PER_ROOM = 6;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        private List<GameRoom> _activeRooms;
        private List<GameRoom> _closedRooms;
        private int _maxRooms;

        public WaitingRoom(int maxLobbies)
        {
            ComponentType = MessageableComponent.WaitingRoom;
            ComponentClients = new List<ServerConnection>();
            _maxRooms = maxLobbies;

            _activeRooms = new List<GameRoom>();
            _closedRooms = new List<GameRoom>();

            GameRoom.OnRoomStateChanged += GameRoom_OnRoomStateChanged;
            GameRoom.OnRoomClosed += GameRoom_OnRoomClosed;
        }

        private void GameRoom_OnRoomClosed(string id)
        {
            for (int i = 0; i < _activeRooms.Count; i++)
            {
                if (_activeRooms[i].ID == id)
                {
                    var closedRoom = _activeRooms[i];
                    _activeRooms.RemoveAt(i);
                    _closedRooms.Add(closedRoom);
                    return;
                }
            }
        }

        private void GameRoom_OnRoomStateChanged()
        {
            foreach (var connectedClient in ComponentClients)
            {
                connectedClient.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_WaitingRoomFullInfo);
            }
        }

        public void AddClientToWaitingRoom(ServerConnection connection)
        {
            ComponentClients.Add(connection);

            connection.AddServerComponent(this);
            connection.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_WaitingRoomFullInfo);
        }

        public void CreateNewRoom(string roomName)
        {
            if (_activeRooms.Count < _maxRooms)
            {
                GameRoom newRoom = new GameRoom(MAX_PEOPLE_PER_ROOM, roomName);
                _activeRooms.Add(newRoom);
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
            foreach(var room in _activeRooms)
            {
                waitingRoomInfo.Rooms[count] = room.GetRoomInformation();
                count++;
            }

            return waitingRoomInfo;
        }

        public GameRoom GetGameRoomClientIsIn(ServerConnection client)
        {
            for (int i = 0; i < _activeRooms.Count; i++)
            {
                for (int j = 0; j < _activeRooms[i].ComponentClients.Count; j++)
                {
                    if (_activeRooms[i].ComponentClients[j] == client)
                    {
                        return _activeRooms[i];
                    }
                }
            }

            throw new Exception("UNABLE TO FIND CLIENT");
        }

        public GameRoom GetGameRoomFromID(string id)
        {
            for (int i = 0; i < _activeRooms.Count; i++)
            {
                if (_activeRooms[i].ID == id)
                {
                    return _activeRooms[i];
                }
            }

            throw new Exception("UNABLE TO FIND ROOM");
        }

        public void RecieveClientMessage(ServerConnection client, MessageType type, byte[] buffer)
        {
            switch (type)
            {
                case MessageType.WR_ClientRequest_WaitingRoomInfo:
                    {
                        client.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_WaitingRoomFullInfo);
                        break;
                    }

                case MessageType.WR_ClientRequest_CreateRoom:
                {
                    if (_activeRooms.Count < Server.MAX_ROOMS)
                    {
                        CreateNewRoom("TEST NEW ROOM " + _activeRooms.Count);
                        GameRoom_OnRoomStateChanged();
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
                    GameRoom joinedRoom = GetGameRoomFromID(joinPacket.String);
                    if (joinedRoom.ComponentClients.Count < MAX_PEOPLE_PER_ROOM)
                    {
                        client.SendPacketToClient(new StringPacket(joinPacket.String), MessageType.WR_ServerResponse_SuccessJoinRoom);
                        joinedRoom.AddClientToRoom(client);
                        GameRoom_OnRoomStateChanged();
                    }
                    else
                    {
                        client.SendPacketToClient(new BasePacket(), MessageType.WR_ServerResponse_FailJoinRoom);
                    }
                    break;
                }              
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < _activeRooms.Count; i++)
            {
                _activeRooms[i].Update(gameTime);
            }
        }
    }
}
