using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MultiplayerProject.Source;
using System;

namespace MultiplayerProject
{
    public delegate void SimpleDelegate(string str);

    public enum PacketType
    {
        TestPacket
    }

    public struct NetworkPacket
    {
        PacketType Type;
        string Contents;
    }

    public class Application : Game
    {
        private GraphicsDeviceManager   _graphics;
        private SpriteBatch             _spriteBatch;

        private KeyboardState           _currentKeyboardState;
        private KeyboardState           _previousKeyboardState;

        private GamePadState            _currentGamePadState;
        private GamePadState            _previousGamePadState;

        private MouseState              _currentMouseState;
        private MouseState              _previousMouseState;

        private IScene _currentScene;

        private const string hostname = "127.0.0.1";
        private const int port = 4444;

        public Application()
        {
            _graphics = new GraphicsDeviceManager(this);
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
            SimpleClient _client = new SimpleClient();
            MainMenu menu = (MainMenu)_currentScene;

            menu.SetMessage("Attempting to connect...");
            if (_client.Connect(hostname, port))
            {
                menu.SetMessage("Connected...");
                try
                {
                    _client.Run();
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
            Console.WriteLine("fdsasdfssdffs");

            SimpleServer _simpleServer = new SimpleServer(hostname, port);
            _simpleServer.Start();
            //_simpleServer.Stop();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _currentScene.Initalise(Content);

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

            _previousGamePadState = _currentGamePadState;
            _previousKeyboardState = _currentKeyboardState;
            _previousMouseState = _currentMouseState;

            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _currentMouseState = Mouse.GetState();

            _currentScene.ProcessInput(gameTime, _currentKeyboardState, _currentGamePadState, _currentMouseState);
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
