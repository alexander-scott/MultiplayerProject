using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiplayerProject
{
    class SimpleServer
    {
        private TcpListener _tcpListener;

        private static List<Client> _clients = new List<Client>();

        public SimpleServer(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            _tcpListener = new TcpListener(ip, port);
        }

        public void Start()
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

        public void Stop()
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                _clients[i].Stop();
            }

            _tcpListener.Stop();
        }

        internal static void SocketMethod(Client client)
        {
            try
            {
                string receivedMessage;

                client.SendText("Send 0 for available options");

                while ((receivedMessage = client.Reader.ReadLine()) != null)
                {
                    Console.WriteLine("Received...");

                    foreach (Client c in _clients)
                    {
                        c.SendText("Client number " + client.ID + " sent " + receivedMessage + " to the Server!");
                    }

                    if (Int32.TryParse(receivedMessage, out int i))
                    {
                        client.SendText(GetReturnMessage(i));
                    }
                    else
                    {
                        client.SendText(GetReturnMessage(-1));
                    }

                    if (i == 9)
                        break;
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

        private static string GetReturnMessage(int code)
        {
            string returnMessage;

            switch (code)
            {
                case 0:
                    returnMessage = "Send 1, 3, 5 or 7 for a joke. Send 9 to close the connection.";
                    break;
                case 1:
                    returnMessage = "What dog can jump higher than a building? Send 2 for punchline!";
                    break;
                case 2:
                    returnMessage = "Any dog, buildings can't jump!";
                    break;
                case 3:
                    returnMessage = "When do Ducks wake up? Send 4 for punchline!";
                    break;
                case 4:
                    returnMessage = "At the Quack of Dawn!";
                    break;
                case 5:
                    returnMessage = "How do cows do mathematics? Send 6 for punchline!";
                    break;
                case 6:
                    returnMessage = "They use a cow-culator.";
                    break;
                case 7:
                    returnMessage = "How many programmers does it take to screw in a light bulb? Send 8 for punchline!";
                    break;
                case 8:
                    returnMessage = "None, that's a hardware problem.";
                    break;
                case 9:
                    returnMessage = "Bye!";
                    break;
                default:
                    returnMessage = "Invalid Selection";
                    break;
            }

            return returnMessage;
        }
    }
}
