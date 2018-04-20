using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace MultiplayerProject.Source
{
    public class GameRoom : IMessageable
    {
        public static event EmptyDelegate OnRoomStateChanged;
        public static event StringDelegate OnRoomClosed;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        public string RoomName { get; set; }
        public string ID { get; set; }

        private GameRoomState roomState;
        private int _maxConnections;
        private Dictionary<ServerConnection, bool> _clientReadyStatus;

        private GameInstance _gameInstance;
        private ServerLeaderboard _gameLeaderboard;

        public GameRoom(int maxConnections, string name)
        {
            _maxConnections = maxConnections;
            RoomName = name;

            _clientReadyStatus = new Dictionary<ServerConnection, bool>();
            roomState = GameRoomState.Waiting;

            ID = Guid.NewGuid().ToString();
            ComponentClients = new List<ServerConnection>();

            ServerLeaderboard.OnClientLeaveGameRoom += ServerLeaderboard_OnClientLeaveGameRoom;
            ServerLeaderboard.OnStartRematch += ServerLeaderboard_OnStartRematch;
        }

        public void AddClientToRoom(ServerConnection client)
        {
            ComponentClients.Add(client);
            _clientReadyStatus.Add(client, false);
            client.AddServerComponent(this);
        }

        public RoomInformation GetRoomInformation()
        {
            return NetworkPacketFactory.Instance.MakeRoomInformationPacket(RoomName, ID, ComponentClients, GetReadyCount(), roomState);
        }

        public void RecieveClientMessage(ServerConnection client, BasePacket recievedPacket)
        {
            switch ((MessageType)recievedPacket.MessageType)
            {
                case MessageType.WR_ClientRequest_LeaveRoom:
                    {
                        StringPacket leavePacket = (StringPacket)recievedPacket;
                        client.SendPacketToClient(NetworkPacketFactory.Instance.MakeStringPacket(leavePacket.String), MessageType.WR_ServerResponse_SuccessLeaveRoom);
                        RemoveClient(client);
                        Logger.Instance.Info(client.Name + " left " + RoomName);
                        break;
                    }

                case MessageType.GR_ClientRequest_Ready:
                    {
                        // Can this request fail?
                        client.SendPacketToClient(new BasePacket(), MessageType.GR_ServerResponse_SuccessReady);
                        _clientReadyStatus[client] = true;

                        Logger.Instance.Info(client.Name + " readied up in " + RoomName + ". (" + GetReadyCount() + "/" + ComponentClients.Count + ") players ready");

                        if (GetReadyCount() == ComponentClients.Count)
                        {
                            // TODO: Introduce a countdown here

                            LaunchGameInstance();
                            Logger.Instance.Info(RoomName + " has started a game instance");
                        }

                        OnRoomStateChanged();

                        break;
                    }

                case MessageType.GR_ClientRequest_Unready:
                    {
                        client.SendPacketToClient(new BasePacket(), MessageType.GR_ServerResponse_SuccessUnready);
                        _clientReadyStatus[client] = false;

                        Logger.Instance.Info(client.Name + " unreadied in " + RoomName + ". (" + GetReadyCount() + "/" + ComponentClients.Count + ") players ready");

                        OnRoomStateChanged();
                        break;
                    }
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
            _clientReadyStatus.Remove(client);
            OnRoomStateChanged();
        }

        private void LaunchGameInstance()
        {
            roomState = GameRoomState.InSession;

            _gameInstance = new GameInstance(ComponentClients, ID);
            _gameInstance.OnGameCompleted += OnGameCompleted;
        }

        private void OnGameCompleted(BasePacket packet)
        {
            _gameInstance.OnGameCompleted -= OnGameCompleted;

            roomState = GameRoomState.Leaderboards;
            OnRoomStateChanged();

            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].RemoveServerComponent(_gameInstance);
            }

            _gameLeaderboard = new ServerLeaderboard(ComponentClients, ID); 
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

        public void Update(GameTime gameTime)
        {
            if (roomState == GameRoomState.InSession)
            {
                if (_gameInstance == null)
                    return;

                _gameInstance.Update(gameTime);
            }
            else if (roomState == GameRoomState.Leaderboards)
            {
                if (_gameLeaderboard == null)
                    return;

                _gameLeaderboard.Update(gameTime);
            }
        }

        private void ServerLeaderboard_OnStartRematch()
        {
            if (ComponentClients.Count > 0)
            {
                LaunchGameInstance();
                OnRoomStateChanged();
                Logger.Instance.Info(RoomName + " has restarted their game");
            }
        }

        private void ServerLeaderboard_OnClientLeaveGameRoom(ServerConnection client, string roomID)
        {
            client.RemoveServerComponent(this);
            RemoveClient(client);
            Logger.Instance.Info(client.Name + " left " + RoomName);

            if (ComponentClients.Count == 0 && roomID == ID)
            {
                OnRoomClosed(ID);
                OnRoomStateChanged();
            }
        }
    }
}