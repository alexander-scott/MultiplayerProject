using MultiplayerProject.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MultiplayerProject
{
    // protoc -I=. --csharp_out=./ ./networkpackages.proto
    public class ServerConnection
    {
        public Socket ClientSocket;
        public string ID;
        public string Name;

        public NetworkStream Stream;
        public BinaryReader Reader;
        public BinaryWriter Writer;

        private Server _server;

        private Dictionary<MessageableComponent, Thread> _messageableComponents;

        public ServerConnection(Server server, Socket socket)
        {
            _server = server;
            ClientSocket = socket;
            Stream = new NetworkStream(ClientSocket, true);
            Reader = new BinaryReader(Stream, Encoding.UTF8);
            Writer = new BinaryWriter(Stream, Encoding.UTF8);

            ID = Guid.NewGuid().ToString();
            Name = "Test Connection Name";
            _messageableComponents = new Dictionary<MessageableComponent, Thread>();
        }

        public void Start(IMessageable component)
        {
            Thread thread = new Thread(() => ProcessClientMessage(component));
            thread.Start();

            _messageableComponents.Add(component.ComponentType, thread);
        }

        public void Stop(IMessageable component)
        {
            Thread thread = _messageableComponents[component.ComponentType];
            thread.Abort();
            _messageableComponents.Remove(component.ComponentType);
        }

        public void StopAll()
        {
            ClientSocket.Close();
            foreach (KeyValuePair<MessageableComponent, Thread> entry in _messageableComponents)
            {
                entry.Value.Abort();
            }
            _messageableComponents.Clear();
        }

        public void SendPacketToClient(BasePacket packet, MessageType type)
        {
            byte[] bytes = packet.PackMessage(type);
            Writer.Write(Convert.ToBase64String(bytes));
            Writer.Flush();
        }

        private void ProcessClientMessage(IMessageable component)
        {
            component.ProcessClientMessage(this);
        }
    }
}
