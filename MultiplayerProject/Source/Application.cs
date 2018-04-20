using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MultiplayerProject.Source;
using System;

namespace MultiplayerProject
{
    public class Application : Game
    {
        #region Values

        public const int WINDOW_HEIGHT = 480;
        public const int WINDOW_WIDTH = 800;

        public const bool APPLY_CLIENT_SIDE_PREDICTION = true;
        public const bool APPLY_SERVER_RECONCILLIATION = true;
        public const bool APPLY_ENTITY_INTERPOLATION = true;

        public const int SERVER_UPDATE_RATE = 60 / 5; // Number of frames between packets
        public const int CLIENT_UPDATE_RATE = 60 / 30;

        public const int PLAYER_STARTING_HEALTH = 100;
        public const float PLAYER_ACCELERATION_SPEED = 12f;
        public const float PLAYER_ROTATION_SPEED = 2f;
        public const float PLAYER_MAX_SPEED = 15f;
        public const float PLAYER_DECELERATION_AMOUNT = 0.95f;

        public const int SCORE_TO_WIN = 3;

        #endregion

        private GraphicsDeviceManager   _graphics;
        private SpriteBatch             _spriteBatch;

        private InputInformation        _inputInformation;

        private MainMenu                _mainMenu;
        private ApplicationType         _appType;

        private ServerApplication       _server;
        private ClientApplication       _client;

        private const string hostname = "127.0.0.1";
        private const int port = 4444;

        public Application()
        {
            _graphics = new GraphicsDeviceManager(this);
            _appType = ApplicationType.None;

            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;

            IsMouseVisible = true;
            Content.RootDirectory = "Content";

            MonoGame_Textbox.KeyboardInput.Initialize(this, 500f, 20);
            Logger.Instance.Initalise(true);

            MainMenu.OnServerStartRequested += OnServerStartRequested;
            MainMenu.OnClientStartRequested += OnClientStartRequested;

            ClientApplication.OnRequestToReturnToMainMenu += ReturnToMainMenu;
        }

        protected override void Initialize()
        {
            _mainMenu = new MainMenu();

            _server = new ServerApplication(_graphics.GraphicsDevice, Content);
            _client = new ClientApplication(_graphics.GraphicsDevice, Content);

            base.Initialize(); 
        }

        private void OnClientStartRequested(string playerName)
        {
            _appType = ApplicationType.Client;
            _client.Initalise(hostname, port, playerName);
        }

        private void OnServerStartRequested(string str)
        {
            _appType = ApplicationType.Server;
            _server.Initalise(hostname, port);
            Logger.Instance.Info("Launching new server instance");
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _mainMenu.Initalise(Content, _graphics.GraphicsDevice);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            ProcessInput(gameTime);

            switch(_appType)
            {
                case ApplicationType.None:
                    _mainMenu.Update(gameTime);
                    break;

                case ApplicationType.Client:
                    _client.Update(gameTime);
                    break;

                case ApplicationType.Server:
                    _server.Update(gameTime);
                    break;
            }
            
            base.Update(gameTime);
        }

        private void ProcessInput(GameTime gameTime)
        {
            // This fixes the issue of clicking through windows
            //if (!IsActive)
            //    return;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _inputInformation.PreviousGamePadState = _inputInformation.CurrentGamePadState;
            _inputInformation.PreviousKeyboardState = _inputInformation.CurrentKeyboardState;
            _inputInformation.PreviousMouseState = _inputInformation.CurrentMouseState;

            _inputInformation.CurrentKeyboardState = Keyboard.GetState();
            _inputInformation.CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
            _inputInformation.CurrentMouseState = Mouse.GetState();

            switch (_appType)
            {
                case ApplicationType.None:
                    _mainMenu.ProcessInput(gameTime, _inputInformation);
                    break;

                case ApplicationType.Client:
                    _client.ProcessInput(gameTime, _inputInformation);
                    break;

                case ApplicationType.Server:
                    _server.ProcessInput(gameTime, _inputInformation);
                    break;
            } 
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            _spriteBatch.Begin();

            switch (_appType)
            {
                case ApplicationType.None:
                    _mainMenu.Draw(_spriteBatch);
                    break;

                case ApplicationType.Client:
                    _client.Draw(_spriteBatch);
                    break;

                case ApplicationType.Server:
                    _server.Draw(_spriteBatch);
                    break;
            }

            // Stop drawing
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            switch (_appType)
            {
                case ApplicationType.Client:
                    _client.OnExiting();
                    break;

                case ApplicationType.Server:
                    _server.OnExiting();
                    break;
            }

            base.OnExiting(sender, args);
        }

        private void ReturnToMainMenu()
        {
            _appType = ApplicationType.None;
        }
    }
}
