using System;
using System.Collections.Generic;
using System.IO;

namespace MultiplayerProject.Source
{
    public class GameRoom : IMessageable
    {
        public static event EmptyDelegate OnRoomStateChanged;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        public string RoomName;
        public string ID;

        private bool _isPlaying;
        private int _maxConnections;
        private Dictionary<ServerConnection, bool> _clientReadyStatus;

        private GameInstance _gameInstance;

        public GameRoom(int maxConnections, string name)
        {
            _maxConnections = maxConnections;
            RoomName = name;

            _clientReadyStatus = new Dictionary<ServerConnection, bool>();
            _isPlaying = false;

            ID = Guid.NewGuid().ToString();
            ComponentClients = new List<ServerConnection>();
        }

        public void AddClientToRoom(ServerConnection client)
        {
            ComponentClients.Add(client);
            _clientReadyStatus.Add(client, false);
            client.AddServerComponent(this);
        }

        public RoomInformation GetRoomInformation()
        {
            return new RoomInformation(RoomName, ID, ComponentClients, GetReadyCount(), _isPlaying);
        }

        public void RecieveClientMessage(ServerConnection client, MessageType type, byte[] buffer)
        {
            switch (type)
            {
                case MessageType.WR_ClientRequest_LeaveRoom:
                    {
                        StringPacket leavePacket = buffer.DeserializeFromBytes<StringPacket>();
                        client.SendPacketToClient(new StringPacket(leavePacket.String), MessageType.WR_ServerResponse_SuccessLeaveRoom);
                        RemoveClient(client);
                        break;
                    }

                case MessageType.GR_ClientRequest_Ready:
                    {
                        // Can this request fail?
                        client.SendPacketToClient(new BasePacket(), MessageType.GR_ServerResponse_SuccessReady);
                        _clientReadyStatus[client] = true;

                        OnRoomStateChanged();

                        if (GetReadyCount() == ComponentClients.Count)
                        {
                            // TODO: Introduce a countdown here

                            LaunchGameInstance();
                        }

                        break;
                    }

                case MessageType.GR_ClientRequest_Unready:
                    {
                        client.SendPacketToClient(new BasePacket(), MessageType.GR_ServerResponse_SuccessUnready);
                        _clientReadyStatus[client] = false;

                        OnRoomStateChanged();
                        break;
                    }
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
            _clientReadyStatus.Remove(client);
            client.RemoveServerComponent(this);
            OnRoomStateChanged();
        }

        private void LaunchGameInstance()
        {
            _isPlaying = true;

            _gameInstance = new GameInstance(ComponentClients);
            _gameInstance.OnReturnToGameRoom += _gameInstance_OnReturnToGameRoom;
        }

        private void _gameInstance_OnReturnToGameRoom()
        {
            _gameInstance.OnReturnToGameRoom -= _gameInstance_OnReturnToGameRoom;

            throw new NotImplementedException();
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
    }
}
