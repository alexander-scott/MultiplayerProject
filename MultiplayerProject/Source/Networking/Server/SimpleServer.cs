using MultiplayerProject.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MultiplayerProject
{
    class SimpleServer
    {
        private TcpListener _tcpListener;
        private Thread _thread;

        private static List<Client> _clients = new List<Client>();

        public SimpleServer(string ipAddress, int port)
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
                Console.WriteLine("Connection Made");

                Client client = new Client(socket);
                _clients.Add(client);
                client.Start();
            }
        }

        internal static void SocketMethod(Client client)
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
                                switch (stream.UnPackMessage(messageSize, out byte[] buffer))
                                {
                                    case MessageType.NetworkPacket:
                                        var myClassCopy = buffer.DeserializeFromBytes<NetworkPacket>();
                                        // do stuff with your class
                                        break;
                                    case MessageType.NetworkPacketString:
                                        break;
                                    case MessageType.Message3:
                                        break;
                                    case MessageType.Message4:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
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
    }
}
