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
        public static event EmptyDelegate OnRequestToReturnToMainMenu;

        private IScene _currentScene;

        private Client _client;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _contentManager;

        private bool _sceneLoading;

        private int _width;
        private int _height;

        public ClientApplication(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _client = new Client();
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;

            _width = graphicsDevice.Viewport.Width;
            _height = graphicsDevice.Viewport.Height;

            ClientMessenger.OnServerForcedDisconnect += Client_OnDisconnectedFromServer;
            ClientMessenger.OnLoadNewGame += ClientMessenger_OnLoadNewGame;
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

                    _sceneLoading = true;
                    _currentScene = new WaitingRoomScene();
                    _currentScene.Initalise(_contentManager, _graphicsDevice);
                    _sceneLoading = false;
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
            if (_currentScene == null || _sceneLoading)
                return;

            _currentScene.Update(gameTime);
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            if (_currentScene == null || _sceneLoading)
                return;

            _currentScene.ProcessInput(gameTime, inputInfo);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentScene == null || _sceneLoading)
                return;

            _currentScene.Draw(spriteBatch);
        }

        public void OnExiting()
        {
            // Tell the server we are disconnecting
            _client.SendMessageToServer(new BasePacket(), MessageType.Client_Disconnect);
            _client.Stop();
        }

        private void Client_OnDisconnectedFromServer()
        {
            // The server has disconnected for some reason so we shall return to the main menu
            OnRequestToReturnToMainMenu();

            Console.WriteLine("Disconnected from server");
            _client.Stop(); 
        }

        private void ClientMessenger_OnLoadNewGame(BasePacket packet)
        {
            GameInstanceInformation gameInstance = (GameInstanceInformation)packet;

            _sceneLoading = true;
            _currentScene = new MainGame(_width, _height, gameInstance.PlayerCount, gameInstance.PlayerIDs, gameInstance.PlayerColours, gameInstance.LocalPlayerID, _client);
            _currentScene.Initalise(_contentManager, _graphicsDevice);
            _sceneLoading = false;
        }
    }
}