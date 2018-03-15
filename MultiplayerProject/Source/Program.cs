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
            using (var app = new Application())
                app.Run();
        }
    }
}
