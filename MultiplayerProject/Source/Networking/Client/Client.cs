using MultiplayerProject.Source;
using System;
using System.IO;
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

    public class Client
    {
        public static event WaitingRoomDelegate OnWaitingRoomInformationRecieved;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private BinaryWriter _writer;
        private BinaryReader _reader;
        private Thread _thread;

        public Client()
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

        public void SendMessageToServer(BasePacket packet, MessageType type)
        {
            var bytes = packet.PackMessage(type);
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
                        while (stream.HasValidPackage(out Int32 messageSize))
                        {
                            MessageType type = stream.UnPackMessage(messageSize, out byte[] buffer);
                            RecieveServerResponse(type, buffer);
                        }
                    }
                }
            }
        }

        private void RecieveServerResponse(MessageType messageType, byte[] packetBytes)
        {
            switch (messageType)
            {
                case MessageType.NetworkPacket:
                    var packet = packetBytes.DeserializeFromBytes<NetworkPacket>();
                    Console.WriteLine("Server says: " + packet.SomeArbitaryString);
                    break;

                case MessageType.WR_ServerSend_FullInfo:
                    var waitingRooms = packetBytes.DeserializeFromBytes<WaitingRoomInformation>();
                    OnWaitingRoomInformationRecieved(waitingRooms);
                    break;
            }
        }
    }
}
