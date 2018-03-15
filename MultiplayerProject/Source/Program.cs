using System;

namespace MultiplayerProject
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    { 
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
