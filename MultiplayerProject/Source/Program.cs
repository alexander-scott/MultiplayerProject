using System;

namespace MultiplayerProject
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private const string hostname = "127.0.0.1";
        private const int port = 4444;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //using (var game = new MultiplayerGame())
            //    game.Run();

            Console.WriteLine("Press 1 to be a server, 2 to be a client:");

            string response = Console.ReadLine();
            Int32.TryParse(response, out int parsedInt);

            if (parsedInt == 1)
            {
                SimpleServer _simpleServer = new SimpleServer(hostname, port);

                _simpleServer.Start();

                _simpleServer.Stop();
            }
            else if (parsedInt == 2)
            {
                SimpleClient _client = new SimpleClient();

                if (_client.Connect(hostname, port))
                {
                    Console.WriteLine("Connected...");

                    try
                    {
                        _client.Run();
                    }
                    catch (NotConnectedException e)
                    {
                        Console.WriteLine("Client not Connected");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to connect to: " + hostname + ":" + port);
                }
                Console.Read();
            }
        }
    }
}
