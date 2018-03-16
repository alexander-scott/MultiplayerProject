using MultiplayerProject.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MultiplayerProject
{
    class Server
    {
        private TcpListener _tcpListener;
        private Thread _thread;

        private List<ServerConnection> _clients = new List<ServerConnection>();

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            _tcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            _thread = new Thread(new ThreadStart(ListenForClients));
            _thread.Start();
        }

        public void Stop()
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                _clients[i].Stop();
            }

            _tcpListener.Stop();
            _thread.Abort();
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
                _clients.Add(client);
                client.Start();
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
                            Console.WriteLine("Received...");

                            // if we have a valid package do stuff
                            // this loops until there isnt enough data for a package or empty
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
                client.Stop();
            }
        }

        private void RecieveClientMessage(ServerConnection client, MessageType messageType, byte[] packetBytes)
        {
            switch (messageType)
            {
                case MessageType.NetworkPacketExtended:
                    var packet = packetBytes.DeserializeFromBytes<NetworkPacketExtended>();
                    Console.WriteLine("Client says: " + packet.String);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
