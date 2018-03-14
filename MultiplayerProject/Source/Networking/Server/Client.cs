using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerProject
{
    public class Client
    {
        public Socket ClientSocket;
        public string ID;

        public NetworkStream Stream;
        public StreamReader Reader;
        public StreamWriter Writer;

        private Thread _thread;

        public Client(Socket socket)
        {
            ClientSocket = socket;
            Stream = new NetworkStream(ClientSocket, true);
            Reader = new StreamReader(Stream, Encoding.UTF8);
            Writer = new StreamWriter(Stream, Encoding.UTF8);

            ID = Guid.NewGuid().ToString();
        }

        public void Start()
        {
            _thread = new Thread(new ThreadStart(SocketMethod));
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

        public void SendText(string text)
        {
            Writer.WriteLine(text);
            Writer.Flush();
        }

        private void SocketMethod()
        {
            SimpleServer.SocketMethod(this);
        }
    }
}
