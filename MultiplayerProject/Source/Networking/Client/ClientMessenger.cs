using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class ClientMessenger
    {
        public static event EmptyDelegate OnServerForcedDisconnect;
        public static event WaitingRoomDelegate OnWaitingRoomInformationRecieved;
        public static event StringDelegate OnRoomSuccessfullyJoined;
        public static event StringDelegate OnRoomSuccessfullyLeft;
        public static event EmptyDelegate OnRoomSuccessfullyReady;
        public static event EmptyDelegate OnRoomSuccessfullyUnready;

        public static event GameRoomDelegate OnLoadNewGame;

        public static event PlayerUpdateDelegate OnRecievedPlayerUpdatePacket;
        public static event PlayerFiredDelegate OnRecievedPlayerFiredPacket;

        private Client _client;

        public ClientMessenger(Client client)
        {
            _client = client;

            WaitingRoomScene.OnNewGameRoomClicked += WaitingRoomScene_OnNewRoomClicked;
            WaitingRoomScene.OnJoinGameRoom += WaitingRoomScene_OnJoinRoom;
            WaitingRoomScene.OnLeaveGameRoom += WaitingRoomScene_OnLeaveRoom;
            WaitingRoomScene.OnClientReady += WaitingRoomScene_OnReady;
            WaitingRoomScene.OnClientUnready += WaitingRoomScene_OnUnready;
        }

        private void WaitingRoomScene_OnUnready()
        {
            _client.SendMessageToServer(new BasePacket(), MessageType.GR_ClientRequest_Unready);
        }

        private void WaitingRoomScene_OnReady()
        {
            _client.SendMessageToServer(new BasePacket(), MessageType.GR_ClientRequest_Ready);
        }

        private void WaitingRoomScene_OnLeaveRoom(string lobbyID)
        {
            _client.SendMessageToServer(new StringPacket(lobbyID), MessageType.WR_ClientRequest_LeaveRoom);
        }

        private void WaitingRoomScene_OnJoinRoom(string lobbyID)
        {
            _client.SendMessageToServer(new StringPacket(lobbyID), MessageType.WR_ClientRequest_JoinRoom);
        }

        private void WaitingRoomScene_OnNewRoomClicked()
        {
            _client.SendMessageToServer(new BasePacket(), MessageType.WR_ClientRequest_CreateRoom);
        }

        public void RecieveServerResponse(MessageType messageType, byte[] packetBytes)
        {
            switch (messageType)
            {
                case MessageType.Server_Disconnect:
                    OnServerForcedDisconnect();
                    break;

                case MessageType.WR_ServerSend_WaitingRoomFullInfo:
                    var waitingRooms = packetBytes.DeserializeFromBytes<WaitingRoomInformation>();
                    OnWaitingRoomInformationRecieved(waitingRooms);
                    break;

                case MessageType.WR_ServerResponse_FailJoinRoom:
                    Console.WriteLine("FAILED TO JOIN ROOM");
                    break;

                case MessageType.WR_ServerResponse_FailCreateRoom:
                    Console.WriteLine("FAILED TO CREATE ROOM");
                    break;

                case MessageType.WR_ServerResponse_SuccessJoinRoom:
                {
                    StringPacket lobbyID = packetBytes.DeserializeFromBytes<StringPacket>();
                    OnRoomSuccessfullyJoined(lobbyID.String);
                    break;
                }

                case MessageType.WR_ServerResponse_SuccessLeaveRoom:
                {
                    StringPacket lobbyID = packetBytes.DeserializeFromBytes<StringPacket>();
                    OnRoomSuccessfullyLeft(lobbyID.String);
                    break;
                }

                case MessageType.GR_ServerResponse_SuccessReady:
                    {
                        OnRoomSuccessfullyReady();
                        break;
                    }

                case MessageType.GR_ServerResponse_SuccessUnready:
                    {
                        OnRoomSuccessfullyUnready();
                        break;
                    }

                case MessageType.GI_ServerSend_LoadNewGame:
                    {
                        OnLoadNewGame(packetBytes.DeserializeFromBytes<GameInstanceInformation>());
                        break;
                    }

                case MessageType.GI_ServerSend_UpdateRemotePlayer:
                    { 
                        var playerPacket = packetBytes.DeserializeFromBytes<PlayerUpdatePacket>();
                        OnRecievedPlayerUpdatePacket(playerPacket);
                        break;
                    }

                case MessageType.GI_ServerSend_RemotePlayerFiredPacket:
                    {
                        var playerPacket = packetBytes.DeserializeFromBytes<PlayerFiredPacket>();
                         OnRecievedPlayerFiredPacket(playerPacket);
                        break;
                    }
            }
        }
    }
}
