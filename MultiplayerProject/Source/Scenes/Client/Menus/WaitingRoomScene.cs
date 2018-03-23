using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
    public class GameRoomUIItem
    {
        public Vector2 Position;
        public Rectangle Rect;
        public string Text;
        public Color BorderColour;
        public int BorderWidth;
    }

    public enum WaitingRoomState
    {
        NotInRoomAbleToCreate,
        NotInRoomUnableToCreate,
        InRoomWaitingForPlayers,
        InRoomNotReady,
        InRoomReady,
    }

    public class WaitingRoomScene : IScene
    {
        public static event EmptyDelegate OnNewGameRoomClicked;
        public static event StringDelegate OnJoinGameRoom;
        public static event StringDelegate OnLeaveGameRoom;
        public static event EmptyDelegate OnClientReady;
        public static event EmptyDelegate OnClientUnready;

        private WaitingRoomInformation _waitingRoom;

        private string _joinedRoomID;
        private WaitingRoomState _state;
        private bool _readyToPlay;

        private SpriteFont _font;
        private GraphicsDevice _device;
        private bool _waitingForResponseFromServer;

        // Title
        private Vector2 _titlePosition;
        private const string _titleText = "WAITING ROOM";

        // Bottom button
        private Vector2 _buttonPosition;
        private Rectangle _buttonRect;
        private Color _buttonColour;
        private string _buttonText = "CREATE NEW ROOM";

        // Rooms
        private List<GameRoomUIItem> _roomUIItems;
        private const int _roomStartYPos = 50;

        public WaitingRoomScene()
        {
            _waitingForResponseFromServer = false;

            _state = WaitingRoomState.NotInRoomAbleToCreate;

            _roomUIItems = new List<GameRoomUIItem>();

            ClientMessenger.OnWaitingRoomInformationRecieved += Client_OnWaitingRoomInformationRecieved;
            ClientMessenger.OnRoomSuccessfullyJoined += ClientMessageReciever_OnRoomSuccessfullyJoined;
            ClientMessenger.OnRoomSuccessfullyLeft += ClientMessageReciever_OnRoomSuccessfullyLeft;
            ClientMessenger.OnRoomSuccessfullyReady += ClientMessenger_OnRoomSuccessfullyReady;
            ClientMessenger.OnRoomSuccessfullyUnready += ClientMessenger_OnRoomSuccessfullyUnready;
        }

        private void ClientMessenger_OnRoomSuccessfullyUnready()
        {
            _readyToPlay = false;
            _waitingForResponseFromServer = false;
            _state = WaitingRoomState.InRoomNotReady;
        }

        private void ClientMessenger_OnRoomSuccessfullyReady()
        {
            _readyToPlay = true;
            _waitingForResponseFromServer = false;
            _state = WaitingRoomState.InRoomReady;
        }

        private void ClientMessageReciever_OnRoomSuccessfullyLeft(string str)
        {
            _joinedRoomID = "";
            _waitingForResponseFromServer = false;
        }

        private void ClientMessageReciever_OnRoomSuccessfullyJoined(string str)
        {
            _joinedRoomID = str;
            _waitingForResponseFromServer = false;
        }

        private void Client_OnWaitingRoomInformationRecieved(WaitingRoomInformation waitingRoom)
        {
            _waitingRoom = waitingRoom;
            _roomUIItems.Clear();

            RoomInformation joinedRoom = null;
            if (_waitingRoom != null)
            {
                int startYPos = _roomStartYPos;
                foreach (var room in _waitingRoom.Rooms)
                {
                    GameRoomUIItem uiItem = new GameRoomUIItem();
                    uiItem.Rect = new Rectangle(50, startYPos, 500, 50);
                    if (!room.IsPlaying)
                        uiItem.Text = room.RoomName + " : " + room.ConnectionCount + "/" + WaitingRoom.MAX_PEOPLE_PER_ROOM + " Players";
                    else
                        uiItem.Text = room.RoomName + " : PLAYING";

                    Vector2 size = _font.MeasureString(uiItem.Text);
                    float xScale = (uiItem.Rect.Width / size.X);
                    float yScale = (uiItem.Rect.Height / size.Y);
                    float scale = Math.Min(xScale, yScale);

                    // Figure out the location to absolutely-center it in the boundaries rectangle.
                    int strWidth = (int)Math.Round(size.X * scale);
                    int strHeight = (int)Math.Round(size.Y * scale);

                    // Set Position
                    uiItem.Position = new Vector2
                    {
                        X = (((uiItem.Rect.Width - strWidth) / 2) + uiItem.Rect.X),
                        Y = (((uiItem.Rect.Height - strHeight) / 2) + uiItem.Rect.Y)
                    };

                    // Set border options
                    uiItem.BorderColour = Color.Blue;
                    uiItem.BorderWidth = 1;

                    _roomUIItems.Add(uiItem);
                    startYPos += 50;

                    if (room.RoomID == _joinedRoomID)
                    {
                        joinedRoom = room;
                    }
                }
            }

            if (joinedRoom != null)
            {
                if (joinedRoom.ConnectionCount == 1)
                {
                    _buttonText = "WAITING FOR MORE PLAYERS";
                    _state = WaitingRoomState.InRoomWaitingForPlayers;
                    _readyToPlay = false;
                }
                else
                {
                    if (!_readyToPlay)
                    {
                        _buttonText = "CLICK TO READY (" + joinedRoom.ReadyCount + "/" + joinedRoom.ConnectionCount + ")";
                        _state = WaitingRoomState.InRoomNotReady;
                    }
                    else
                    {
                        _buttonText = "READY TO PLAY! (" + joinedRoom.ReadyCount + "/" + joinedRoom.ConnectionCount + ")";
                        _state = WaitingRoomState.InRoomReady;
                    }
                }
            }
            else
            {
                if (_roomUIItems.Count < Server.MAX_ROOMS)
                {
                    _buttonText = "CREATE NEW ROOM";
                    _state = WaitingRoomState.NotInRoomAbleToCreate;
                }
                else
                {
                    _buttonText = "UNABLE TO CREATE NEW ROOM";
                    _state = WaitingRoomState.NotInRoomUnableToCreate;
                }
            }

            ReformatButton();
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            _device = graphicsDevice;

            _titlePosition = new Vector2(Application.WINDOW_WIDTH / 2, 0);
            _titlePosition.X -= (_font.MeasureString(_titleText).X / 2);

            ReformatButton();
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            if (_waitingForResponseFromServer)
                return;

            CheckBottomButtonClicked(inputInfo);

            CheckRoomClicked(inputInfo);
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw title
            spriteBatch.DrawString(_font, _titleText, _titlePosition, Color.White);

            // Draw new room button
            spriteBatch.DrawString(_font, _buttonText, _buttonPosition, Color.White);
            Texture2D newRoomBtnTexture = new Texture2D(_device, _buttonRect.Width, _buttonRect.Height);
            newRoomBtnTexture.CreateBorder(1, _buttonColour);
            spriteBatch.Draw(newRoomBtnTexture, _buttonPosition, Color.White);

            if (_roomUIItems.Count != 0)
            {
                for (int i = 0; i < _roomUIItems.Count; i++)
                {
                    // Draw room ui item string
                    spriteBatch.DrawString(_font, _roomUIItems[i].Text, _roomUIItems[i].Position, Color.White);

                    // Draw room ui item border
                    Texture2D texture = new Texture2D(_device, _roomUIItems[i].Rect.Width, _roomUIItems[i].Rect.Height);
                    texture.CreateBorder(_roomUIItems[i].BorderWidth, _roomUIItems[i].BorderColour);
                    spriteBatch.Draw(texture, _roomUIItems[i].Position, Color.White);
                }
            }
        }

        private void ReformatButton()
        {
            _buttonPosition = new Vector2(Application.WINDOW_WIDTH / 2, Application.WINDOW_HEIGHT - (_font.MeasureString(_buttonText).Y));
            _buttonPosition.X -= (_font.MeasureString(_buttonText).X / 2);
            _buttonRect = new Rectangle((int)_buttonPosition.X, (int)_buttonPosition.Y,
                (int)_font.MeasureString(_buttonText).X, (int)_font.MeasureString(_buttonText).Y);
            _buttonColour = Color.Blue;
        }

        private void CheckRoomClicked(InputInformation inputInfo)
        {
            switch (_state)
            {
                case WaitingRoomState.NotInRoomUnableToCreate:
                case WaitingRoomState.NotInRoomAbleToCreate:
                    {
                        if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            for (int i = 0; i < _roomUIItems.Count; i++)
                            {
                                if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position) && !_waitingRoom.Rooms[i].IsPlaying)
                                {
                                    OnJoinGameRoom(_waitingRoom.Rooms[i].RoomID);
                                    _waitingForResponseFromServer = true;
                                }
                            }
                        }
                        else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            for (int i = 0; i < _roomUIItems.Count; i++)
                            {
                                if (_waitingRoom.Rooms[i].IsPlaying)
                                {
                                    _roomUIItems[i].BorderColour = Color.Red;
                                }
                                else if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                                {
                                    _roomUIItems[i].BorderColour = Color.LightGreen;
                                }
                                else
                                {
                                    _roomUIItems[i].BorderColour = Color.Blue;
                                }
                            }
                        }
                        break;
                    }

                case WaitingRoomState.InRoomWaitingForPlayers:
                case WaitingRoomState.InRoomNotReady:
                    {
                        if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            for (int i = 0; i < _roomUIItems.Count; i++)
                            {
                                if (_waitingRoom.Rooms[i].RoomID == _joinedRoomID)
                                {
                                    if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                                    {
                                        OnLeaveGameRoom(_waitingRoom.Rooms[i].RoomID);
                                        _waitingForResponseFromServer = true;
                                    }
                                }
                            }
                        }
                        else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            for (int i = 0; i < _roomUIItems.Count; i++)
                            {
                                if (_waitingRoom.Rooms[i].RoomID == _joinedRoomID)
                                {
                                    if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                                    {
                                        _roomUIItems[i].BorderColour = Color.Orange;
                                    }
                                    else
                                    {
                                        _roomUIItems[i].BorderColour = Color.LightGreen;
                                    }
                                }
                                else
                                {
                                    if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                                    {
                                        _roomUIItems[i].BorderColour = Color.Red;
                                    }
                                    else
                                    {
                                        _roomUIItems[i].BorderColour = Color.Blue;
                                    }
                                }
                            }
                        }
                        break;
                    }

                case WaitingRoomState.InRoomReady:
                    {
                        for (int i = 0; i < _roomUIItems.Count; i++)
                        {
                            if (_waitingRoom.Rooms[i].RoomID == _joinedRoomID)
                            {
                                _roomUIItems[i].BorderColour = Color.LightGreen;
                            }
                            else
                            {
                                _roomUIItems[i].BorderColour = Color.Blue;
                            }
                        }
                        break;
                    }
            }
        }

        private void CheckBottomButtonClicked(InputInformation inputInfo)
        {
            switch (_state)
            {
                case WaitingRoomState.NotInRoomAbleToCreate:
                    {
                        // Is it possible to create a new room? Must be less rooms than max and this client can't currently be in a room
                        if (_roomUIItems.Count < Server.MAX_ROOMS && string.IsNullOrEmpty(_joinedRoomID))
                        {
                            // If mouse has been clicked
                            if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                            {
                                if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                                {
                                    // New room valid click
                                    OnNewGameRoomClicked();
                                }
                            }
                            else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // Else if mouse is hovering
                            {
                                if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                                {
                                    _buttonColour = Color.LightGreen;
                                }
                                else
                                {
                                    _buttonColour = Color.Blue;
                                }
                            }
                        }
                        else
                        {
                            _buttonColour = Color.Red;
                        }
                        break;
                    }

                case WaitingRoomState.NotInRoomUnableToCreate:
                    {
                        _buttonColour = Color.Red;
                        break;
                    }

                case WaitingRoomState.InRoomWaitingForPlayers:
                    {
                        _buttonColour = Color.Red;
                        break;
                    }

                case WaitingRoomState.InRoomNotReady:
                    {
                        if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                            {
                                // ON READY UP
                                OnClientReady();
                                _waitingForResponseFromServer = true;
                            }
                        }
                        else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // Else if mouse is hovering
                        {
                            if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                            {
                                _buttonColour = Color.LightGreen;
                            }
                            else
                            {
                                _buttonColour = Color.Orange;
                            }
                        }
                        else
                        {
                            _buttonColour = Color.Orange;
                        }

                        break;
                    }

                case WaitingRoomState.InRoomReady:
                    {
                        if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                        {
                            if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                            {
                                // ON UNREADY
                                OnClientUnready();
                                _waitingForResponseFromServer = true;
                            }
                        }
                        else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // Else if mouse is hovering
                        {
                            if (_buttonRect.Contains(inputInfo.CurrentMouseState.Position))
                            {
                                _buttonColour = Color.Orange;
                            }
                            else
                            {
                                _buttonColour = Color.LightGreen;
                            }
                        }
                        else
                        {
                            _buttonColour = Color.LightGreen;
                        }
                        break;
                    }
            }
        }
    }
}
