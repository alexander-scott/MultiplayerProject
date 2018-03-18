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
        public static event StringDelegate OnRoomSuccessfullyJoined;
        public static event StringDelegate OnRoomSuccessfullyLeft;

        private Client _client;

        public ClientMessageReciever(Client client)
        {
            _client = client;

            WaitingRoomScene.OnNewGameRoomClicked += WaitingRoomScene_OnNewRoomClicked;
            WaitingRoomScene.OnJoinGameRoom += WaitingRoomScene_OnJoinRoom;
            WaitingRoomScene.OnLeaveGameRoom += WaitingRoomScene_OnLeaveRoom;
        }

        private void WaitingRoomScene_OnLeaveRoom(string roomID)
        {
            _client.SendMessageToServer(new StringPacket(roomID), MessageType.WR_ClientRequest_LeaveRoom);
        }

        private void WaitingRoomScene_OnJoinRoom(string roomID)
        {
            _client.SendMessageToServer(new StringPacket(roomID), MessageType.WR_ClientRequest_JoinRoom);
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

                case MessageType.WR_ServerSend_FullInfo:
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
                    StringPacket roomID = packetBytes.DeserializeFromBytes<StringPacket>();
                    OnRoomSuccessfullyJoined(roomID.String);
                    break;
                }

                case MessageType.WR_ServerResponse_SuccessLeaveRoom:
                {
                    StringPacket roomID = packetBytes.DeserializeFromBytes<StringPacket>();
                    OnRoomSuccessfullyLeft(roomID.String);
                    break;
                }
            }
        }
    }
}
