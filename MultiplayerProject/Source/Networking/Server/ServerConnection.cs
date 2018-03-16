using MultiplayerProject.Source;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MultiplayerProject
{
    // protoc -I=. --csharp_out=./ ./networkpackages.proto
    class ServerConnection
    {
        public Socket ClientSocket;
        public string ID;

        public NetworkStream Stream;
        public BinaryReader Reader;
        public BinaryWriter Writer;

        private Thread _thread;
        private Server _server;

        public ServerConnection(Server server, Socket socket)
        {
            _server = server;
            ClientSocket = socket;
            Stream = new NetworkStream(ClientSocket, true);
            Reader = new BinaryReader(Stream, Encoding.UTF8);
            Writer = new BinaryWriter(Stream, Encoding.UTF8);

            ID = Guid.NewGuid().ToString();
        }

        public void Start()
        {
            _thread = new Thread(new ThreadStart(ProcessClientMessage));
            _thread.Start();
        }

        public void Stop()
        {
            ClientSocket.Close();
            if (_thread.IsAlive)
            {
                _thread.Abort();
            }
        }

        public void SendPacketToClient(NetworkPacket packet, MessageType type)
        {
            byte[] bytes = packet.PackMessage(type);
            Writer.Write(Convert.ToBase64String(bytes));
            Writer.Flush();
        }

        private void ProcessClientMessage()
        {
            _server.ProcessClientMessage(this);
        }
    }
}
