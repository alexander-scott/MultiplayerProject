using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class ClientMessageReciever
    {
        public static event EmptyDelegate OnServerForcedDisconnect;
        public static event WaitingRoomDelegate OnWaitingRoomInformationRecieved;
        public static event StringDelegate OnLobbySuccessfullyJoined;
        public static event StringDelegate OnLobbySuccessfullyLeft;

        private Client _client;

        public ClientMessageReciever(Client client)
        {
            _client = client;

            WaitingRoomScene.OnNewLobbyClicked += WaitingRoomScene_OnNewLobbyClicked;
            WaitingRoomScene.OnJoinLobby += WaitingRoomScene_OnJoinLobby;
            WaitingRoomScene.OnLeaveLobby += WaitingRoomScene_OnLeaveLobby;
        }

        private void WaitingRoomScene_OnLeaveLobby(string lobbyID)
        {
            _client.SendMessageToServer(new StringPacket(lobbyID), MessageType.WR_ClientRequest_LeaveLobby);
        }

        private void WaitingRoomScene_OnJoinLobby(string lobbyID)
        {
            _client.SendMessageToServer(new StringPacket(lobbyID), MessageType.WR_ClientRequest_JoinLobby);
        }

        private void WaitingRoomScene_OnNewLobbyClicked()
        {
            _client.SendMessageToServer(new BasePacket(), MessageType.WR_ClientRequest_CreateLobby);
        }

        public void RecieveServerResponse(MessageType messageType, byte[] packetBytes)
        {
            switch (messageType)
            {
                case MessageType.Server_Disconnect:
                    OnServerForcedDisconnect();
                    break;

                case MessageType.WR_ServerSend_FullInfo:
                    var waitingRooms = packetBytes.DeserializeFromBytes<WaitingRoomInformation>();
                    OnWaitingRoomInformationRecieved(waitingRooms);
                    break;

                case MessageType.WR_ServerResponse_FailJoinLobby:
                    Console.WriteLine("FAILED TO JOIN ROOM");
                    break;

                case MessageType.WR_ServerResponse_FailCreateLobby:
                    Console.WriteLine("FAILED TO CREATE ROOM");
                    break;

                case MessageType.WR_ServerResponse_SuccessJoinLobby:
                {
                    StringPacket lobbyID = packetBytes.DeserializeFromBytes<StringPacket>();
                    OnLobbySuccessfullyJoined(lobbyID.String);
                    break;
                }

                case MessageType.WR_ServerResponse_SuccessLeaveLobby:
                {
                    StringPacket lobbyID = packetBytes.DeserializeFromBytes<StringPacket>();
                    OnLobbySuccessfullyLeft(lobbyID.String);
                    break;
                }
            }
        }
    }
}
