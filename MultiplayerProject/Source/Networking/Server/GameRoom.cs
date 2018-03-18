using System;
using System.Collections.Generic;
using System.IO;

namespace MultiplayerProject.Source
{
    public class GameRoom : IMessageable
    {
        public string RoomName;
        public string ID;

        private int _maxConnections;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        public GameRoom(int maxConnections, string name)
        {
            _maxConnections = maxConnections;
            RoomName = name;

            ID = Guid.NewGuid().ToString();
            ComponentClients = new List<ServerConnection>();
        }

        public void AddClientToRoom(ServerConnection client)
        {
            ComponentClients.Add(client);
            client.AddServerComponent(this);
        }

        public RoomInformation GetRoomInformation()
        {
            return new RoomInformation(RoomName, ID, ComponentClients);
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
            
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
            client.RemoveServerComponent(this);
        }
    }
}
