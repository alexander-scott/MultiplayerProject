using System;
using System.Collections.Generic;
using System.IO;

namespace MultiplayerProject.Source
{
    public class Lobby : IMessageable
    {
        public string LobbyName;
        public string ID;

        private int _maxConnections;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        public Lobby(int maxConnections, string name)
        {
            _maxConnections = maxConnections;
            LobbyName = name;

            ID = Guid.NewGuid().ToString();
            ComponentClients = new List<ServerConnection>();
        }

        public void AddClientToLobby(ServerConnection client)
        {
            ComponentClients.Add(client);

            client.AddServerComponent(this);
        }

        public LobbyInformation GetLobbyInformation()
        {
            return new LobbyInformation(LobbyName, ID, ComponentClients);
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
                client.RemoveServerComponent(this);
            }
        }

        public void RecieveClientMessage(ServerConnection client, MessageType type, byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
        }
    }
}
