using MultiplayerProject.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MultiplayerProject
{
    class NotConnectedException : Exception
    {
        public NotConnectedException() : base("TcpClient not connected.")
        { }

        public NotConnectedException(string message) : base(message)
        { }
    }

    class SimpleClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private BinaryWriter _writer;
        private BinaryReader _reader;
        private Thread _thread;

        public SimpleClient()
        {
            _tcpClient = new TcpClient();
        }

        public bool Connect(string hostname, int port)
        {
            try
            {
                _tcpClient.Connect(hostname, port);
                _stream = _tcpClient.GetStream();
                _writer = new BinaryWriter(_stream, Encoding.UTF8);
                _reader = new BinaryReader(_stream, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }

            return true;
        }

        public void Run()
        {
            if (!_tcpClient.Connected)
                throw new NotConnectedException();

            try
            {
                // Start listening for messages from the server
                _thread = new Thread(new ThreadStart(ProcessServerResponse));
                _thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Error: " + e.Message);
            }
        }

        public void Stop()
        {
            _tcpClient.Close();
        }

        public void SendMessageToServer(NetworkPacket packet)
        {
            var bytes = packet.PackMessage(MessageType.NetworkPacket);
            _writer.Write(Convert.ToBase64String(bytes));
            _writer.Flush();
        }

        private void ProcessServerResponse()
        {
            while (true)
            {
                string message;
                while ((message = _reader.ReadString()) != null)
                {
                    byte[] bytes = Convert.FromBase64String(message);
                    using (var stream = new MemoryStream(bytes))
                    {
                        // if we have a valid package do stuff
                        // this loops until there isnt enough data for a package or empty
                        while (stream.HasValidPackage(out Int32 messageSize))
                        {
                            switch (stream.UnPackMessage(messageSize, out byte[] buffer))
                            {
                                case MessageType.NetworkPacket:
                                    // do stuff with your class
                                    break;
                                case MessageType.NetworkPacketString:
                                    var myClassCopy = buffer.DeserializeFromBytes<NetworkPacketString>();
                                    Console.WriteLine("Server says: " + myClassCopy.String);
                                    Console.WriteLine();
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
    }

    public static class StreamHelpers
    {
        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }
    }
}
