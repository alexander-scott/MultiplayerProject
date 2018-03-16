using MultiplayerProject.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MultiplayerProject
{
    public class Server : IMessageable
    {
        public const int MAX_LOBBIES = 10;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> Clients { get; set; }

        private TcpListener _tcpListener;
        private Thread _listenForClientsThread;

        private WaitingRoom _waitingRoom;

        public Server(string ipAddress, int port)
        {
            // Implement IMessageable
            ComponentType = MessageableComponent.BaseServer;
            Clients = new List<ServerConnection>();

            // Setup TCP Listener
            IPAddress ip = IPAddress.Parse(ipAddress);
            _tcpListener = new TcpListener(ip, port);

            // Create waiting room for connections
            _waitingRoom = new WaitingRoom(MAX_LOBBIES);
        }

        public void Start()
        {
            // Start listening for incomming connections/clients
            _listenForClientsThread = new Thread(new ThreadStart(ListenForClients));
            _listenForClientsThread.Start();
        }

        public void Stop()
        {
            // Stop every clients connection to any component of the server
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].StopAll();
            }

            _tcpListener.Stop();
            _listenForClientsThread.Abort();
        }

        private void ListenForClients()
        {
            _tcpListener.Start();

            Console.WriteLine("Listening...");

            while (true)
            {
                Socket socket = _tcpListener.AcceptSocket();
                Console.WriteLine("New Connection Made");

                ServerConnection client = new ServerConnection(this, socket);
                Clients.Add(client);
                client.Start(this);

                _waitingRoom.AddToWaitingRoom(client);
            }
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

        private void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            // The only packets we should look for recieving here are disconnect or exit messages. Or perhaps info like round trip time or ping time
            switch (messageType)
            {
                case MessageType.NetworkPacket:
                    var packet = packetBytes.DeserializeFromBytes<NetworkPacket>();
                    Console.WriteLine("Client says: " + packet.SomeArbitaryString);
                    break;
            }
        }
    }
}
