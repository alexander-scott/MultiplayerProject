using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class ClientApplication
    {
        private IScene _currentScene;

        private Client _client;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _contentManager;

        public ClientApplication(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _client = new Client();
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;

            ClientMessenger.OnServerForcedDisconnect += Client_OnDisconnectedFromServer;
        }

        public void Initalise(string hostname, int port)
        {
            Console.WriteLine("Attempting to connect...");
            if (_client.Connect(hostname, port))
            {
                Console.WriteLine("Connected...");
                try
                {
                    _client.Run();

                    _currentScene = new WaitingRoomScene(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
                    _currentScene.Initalise(_contentManager, _graphicsDevice);
                }
                catch (NotConnectedException e)
                {
                    Console.WriteLine("Client not Connected: " + e.Message);
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to: " + hostname + ":" + port);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_currentScene == null)
                return;

            _currentScene.Update(gameTime);
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            if (_currentScene == null)
                return;

            _currentScene.ProcessInput(gameTime, inputInfo);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentScene == null)
                return;

            _currentScene.Draw(spriteBatch);
        }

        public void OnExiting()
        {
            _client.SendMessageToServer(new BasePacket(), MessageType.Client_Disconnect);
        }

        private void Client_OnDisconnectedFromServer()
        {
            _currentScene = new DisconnectedFromServerScene(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            _currentScene.Initalise(_contentManager, _graphicsDevice);

            Console.WriteLine("Disconnected from server");

            _client.Stop();
        }
    }
}
