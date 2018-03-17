using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MultiplayerProject.Source;
using System;

namespace MultiplayerProject
{
    public class Application : Game
    {
        private GraphicsDeviceManager   _graphics;
        private SpriteBatch             _spriteBatch;

        private InputInformation        _inputInformation;

        private IScene                  _currentScene;

        private const string hostname = "127.0.0.1";
        private const int port = 4444;

        public Application()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            Content.RootDirectory = "Content";

            MainMenu.OnServerStartRequested += OnServerStartRequested;
            MainMenu.OnClientStartRequested += OnClientStartRequested;
        }

        protected override void Initialize()
        {
            _currentScene = new MainMenu(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            base.Initialize();
        }

        private void OnClientStartRequested(string str)
        {
            Client _client = new Client();
            MainMenu menu = (MainMenu)_currentScene;

            Console.WriteLine("Attempting to connect...");
            if (_client.Connect(hostname, port))
            {
                Console.WriteLine("Connected...");
                try
                {
                    _client.Run();
                    _currentScene = new WaitingRoomScene(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                    _currentScene.Initalise(Content, _graphics.GraphicsDevice);
                }
                catch (NotConnectedException e)
                {
                    menu.SetMessage("Client not Connected: " + e.Message);
                }
            }
            else
            {
                menu.SetMessage("Failed to connect to: " + hostname + ":" + port);
            }
        }

        private void OnServerStartRequested(string str)
        {
            MainMenu menu = (MainMenu)_currentScene;
            menu.SetMessage("You are the server.");

            Server _simpleServer = new Server(hostname, port);
            _simpleServer.Start();
            //_simpleServer.Stop();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _currentScene.Initalise(Content, _graphics.GraphicsDevice);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            ProcessInput(gameTime);

            _currentScene.Update(gameTime);

            base.Update(gameTime);
        }

        private void ProcessInput(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _inputInformation.PreviousGamePadState = _inputInformation.CurrentGamePadState;
            _inputInformation.PreviousKeyboardState = _inputInformation.CurrentKeyboardState;
            _inputInformation.PreviousMouseState = _inputInformation.CurrentMouseState;

            _inputInformation.CurrentKeyboardState = Keyboard.GetState();
            _inputInformation.CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
            _inputInformation.CurrentMouseState = Mouse.GetState();

            _currentScene.ProcessInput(gameTime, _inputInformation);
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            _spriteBatch.Begin();

            _currentScene.Draw(_spriteBatch);

            // Stop drawing
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
