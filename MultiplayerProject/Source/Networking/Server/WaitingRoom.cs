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
        public List<ServerConnection> Clients { get; set; }

        private Dictionary<string,Lobby> _activeLobbys;
        private int _maxLobbies;

        public WaitingRoom(int maxLobbies)
        {
            ComponentType = MessageableComponent.WaitingRoom;
            Clients = new List<ServerConnection>();
            _maxLobbies = maxLobbies;
            _activeLobbys = new Dictionary<string, Lobby>();
        }

        public void AddToWaitingRoom(ServerConnection connection)
        {
            Clients.Add(connection);
            connection.Start(this);
            connection.SendPacketToClient(GetWaitingRoomInformation(), MessageType.WR_ServerSend_FullInfo);
        }

        public void CreateNewWaitingRoom()
        {
            if (_activeLobbys.Count < _maxLobbies)
            {
                Lobby newLobby = new Lobby(MAX_PEOPLE_PER_LOBBY, "Test New Lobby");
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

        public void ProcessClientMessage(ServerConnection client)
        {
            try
            {
                while (true)
                {
                    string message;
                    while ((message = client.Reader.ReadString()) != null)
                    {
                        byte[] bytes = Convert.FromBase64String(message);
                        using (var stream = new MemoryStream(bytes))
                        {
                            while (stream.HasValidPackage(out int messageSize))
                            {
                                MessageType type = stream.UnPackMessage(messageSize, out byte[] buffer);
                                RecieveClientMessage(client, type, buffer);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured: " + e.Message);
            }
            finally
            {
                client.Stop(this);
            }
        }

        private void RecieveClientMessage(ServerConnection client, MessageType type, byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
