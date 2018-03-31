using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class ServerApplication
    {
        public static event EmptyDelegate OnRequestToReturnToMainMenu;

        private ServerScene _currentScene;

        private Server _server;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _contentManager;

        public ServerApplication(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _server = new Server();
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;
        }

        public void Initalise(string hostname, int port)
        {
            _server.Start(hostname, port);

            _currentScene = new ServerScene(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            _currentScene.Initalise(_contentManager, _graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            _server.Update(gameTime);

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
            _server.SendMessageToAllClients(new BasePacket(), MessageType.Server_Disconnect);
            _server.Stop();
        }
    }
}
