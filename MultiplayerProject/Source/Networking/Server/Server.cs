using Microsoft.Xna.Framework;
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
        public const int MAX_ROOMS = 6;

        public MessageableComponent ComponentType { get; set; }
        public List<ServerConnection> ComponentClients { get; set; }

        private TcpListener _tcpListener;
        private Thread _listenForClientsThread;

        private WaitingRoom _waitingRoom;

        public Server()
        {
            // Implement IMessageable
            ComponentType = MessageableComponent.BaseServer;
            ComponentClients = new List<ServerConnection>();

            // Create waiting room for connections
            _waitingRoom = new WaitingRoom(MAX_ROOMS);
        }

        public void Start(string ipAddress, int port)
        {
            // Setup TCP Listener
            IPAddress ip = IPAddress.Parse(ipAddress);
            _tcpListener = new TcpListener(ip, port);

            // Start listening for incomming connections/clients
            _listenForClientsThread = new Thread(new ThreadStart(ListenForClients));
            _listenForClientsThread.Start();
        }

        public void Stop()
        {
            // Stop every clients connection to any component of the server
            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].StopAll();
            }

            _tcpListener.Stop();
            _listenForClientsThread.Abort();
        }

        public void SendMessageToAllClients(BasePacket packet, MessageType packetType)
        {
            for (int i = 0; i < ComponentClients.Count; i++)
            {
                ComponentClients[i].SendPacketToClient(packet, packetType);
            }
        }

        private void ListenForClients()
        {
            _tcpListener.Start();

            Console.WriteLine("Listening...");

            while (true)
            {
                Socket socket = _tcpListener.AcceptSocket();
                Console.WriteLine("New Connection Made");

                // Create a new client instance
                ServerConnection client = new ServerConnection(socket);
                ComponentClients.Add(client);

                client.StartListeningForMessages();
                client.AddServerComponent(this);

                // Add this client to the waiting room
                _waitingRoom.AddClientToWaitingRoom(client);
            }
        }

        public void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            // The only packets we should look for recieving here are disconnect or exit messages. Or perhaps info like round trip time or ping time
            switch (messageType)
            {
                case MessageType.Client_Disconnect:
                    client.StopAll();
                    break;
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
            client.RemoveServerComponent(this);
        }

        public void Update(GameTime gameTime)
        {
            _waitingRoom.Update(gameTime);
        }
    }
}
