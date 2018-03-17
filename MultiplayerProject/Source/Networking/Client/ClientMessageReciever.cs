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

        private Client _client;

        public ClientMessageReciever(Client client)
        {
            _client = client;

            WaitingRoomScene.OnNewLobbyClicked += WaitingRoomScene_OnNewLobbyClicked;
        }

        private void WaitingRoomScene_OnNewLobbyClicked()
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
            }
        }
    }
}
