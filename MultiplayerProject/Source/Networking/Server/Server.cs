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

            if (_listenForClientsThread.IsAlive)
                _listenForClientsThread.Abort();
        }

        private void CleanUp()
        {
            _tcpListener.Stop();
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

            Logger.Instance.Info("Listening for connections...");

            try
            {
                while (true)
                {
                    Socket socket = _tcpListener.AcceptSocket();

                    // Create a new client instance
                    ServerConnection client = new ServerConnection(socket);
                    ComponentClients.Add(client);

                    client.StartListeningForMessages();
                    client.AddServerComponent(this);

                    // Add this client to the waiting room
                    _waitingRoom.AddClientToWaitingRoom(client);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Error("Error occured: " + e.Message);
            }
            finally
            {
                CleanUp();
            }
        }

        public void RecieveClientMessage(ServerConnection client, BasePacket recievedPacket)
        {
            // The only packets we should look for recieving here are disconnect or exit messages. Or perhaps info like round trip time or ping time
            switch ((MessageType)recievedPacket.MessageType)
            {
                case MessageType.Client_Disconnect:
                    client.StopAll();
                    break;

                case MessageType.Client_SendPlayerName:
                    StringPacket namePacket = (StringPacket)recievedPacket;
                    client.SetPlayerName(namePacket.String);
                    Logger.Instance.Info("New player connected : " + client.Name);
                    break;
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            ComponentClients.Remove(client);
        }

        public void Update(GameTime gameTime)
        {
            _waitingRoom.Update(gameTime);
        }
    }
}
