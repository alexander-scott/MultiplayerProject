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
        {}

        public NotConnectedException(string message) : base(message)
        {}
    }

    class SimpleClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private StreamWriter _writer;
        private StreamReader _reader;
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
                _writer = new StreamWriter(_stream, Encoding.UTF8);
                _reader = new StreamReader(_stream, Encoding.UTF8);
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
            
        }

        private void ProcessServerResponse()
        {
            while(true)
            {
                Console.WriteLine("Server says: " + _reader.ReadLine());
                Console.WriteLine();
            }
        }
    }
}
