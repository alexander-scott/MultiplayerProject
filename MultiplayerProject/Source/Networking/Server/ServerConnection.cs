using MultiplayerProject.Source;
using ProtoBuf;
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

        private Thread _thread;

        private List<IMessageable> _messageableComponents;

        public ServerConnection(Socket socket)
        {
            ClientSocket = socket;
            Stream = new NetworkStream(ClientSocket, true);
            Reader = new BinaryReader(Stream, Encoding.UTF8);
            Writer = new BinaryWriter(Stream, Encoding.UTF8);

            ID = Guid.NewGuid().ToString();
            Name = "UNNAMED PLAYER";
            _messageableComponents = new List<IMessageable>();
        }

        public void SetPlayerName(string name)
        {
            Name = name;
        }

        public void AddServerComponent(IMessageable component)
        {
            _messageableComponents.Add(component);
        }

        public void RemoveServerComponent(IMessageable component)
        {
            _messageableComponents.Remove(component);
        }

        public void StartListeningForMessages()
        {
            _thread = new Thread(new ThreadStart(ProcessClientMessage));
            _thread.Start();
        }

        public void StopAll()
        {
            _thread.Abort();
        }

        private void CleanUp()
        {
            ClientSocket.Close();
            for (int i = 0; i < _messageableComponents.Count; i++)
            {
                _messageableComponents[i].RemoveClient(this);
            }
            _messageableComponents.Clear();
        }

        public void SendPacketToClient(BasePacket packet, MessageType type)
        {
            packet.SendDate = DateTime.UtcNow;
            packet.MessageType = (int)type;

            Serializer.SerializeWithLengthPrefix(Writer.BaseStream, packet, PrefixStyle.Base128);
            Writer.Flush();
        }

        private void ProcessClientMessage()
        {
            try
            {
                using (Stream)
                {
                    while (true)
                    {
                        BasePacket packet = Serializer.DeserializeWithLengthPrefix<BasePacket>(Stream, PrefixStyle.Base128);
                        if (packet != null)
                        {
                            for (int i = 0; i < _messageableComponents.Count; i++)
                            {
                                _messageableComponents[i].RecieveClientMessage(this, packet);
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
                CleanUp();
            }
        }
    }
}
