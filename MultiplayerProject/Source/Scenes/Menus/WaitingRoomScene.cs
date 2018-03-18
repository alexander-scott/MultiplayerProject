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

    public class WaitingRoomScene :  IScene
    {
        public static event EmptyDelegate OnNewGameRoomClicked;
        public static event StringDelegate OnJoinGameRoom;
        public static event StringDelegate OnLeaveGameRoom;

        public int Width { get; set; }
        public int Height { get; set; }

        private WaitingRoomInformation _waitingRoom;

        private string _joinedRoomID;

        private SpriteFont _font;
        private GraphicsDevice _device;
        private bool _waitingForResponseFromServer;

        // Title
        private Vector2 _titlePosition;
        private const string _titleText = "WAITING ROOM";

        // New room button
        private Vector2 _newRoomButtonPosition;
        private Rectangle _newRoomButtonRect;
        private Color _newRoomButtonColor;
        private const string _newRoomButtonText = "CREATE NEW ROOM";

        // Rooms
        private List<GameRoomUIItem> _roomUIItems;
        private const int _roomStartYPos = 50;

        public WaitingRoomScene(int width, int height)
        {
            Width = width;
            Height = height;

            _waitingForResponseFromServer = false;

            _roomUIItems = new List<GameRoomUIItem>();

            ClientMessageReciever.OnWaitingRoomInformationRecieved += Client_OnWaitingRoomInformationRecieved;
            ClientMessageReciever.OnRoomSuccessfullyJoined += ClientMessageReciever_OnRoomSuccessfullyJoined;
            ClientMessageReciever.OnRoomSuccessfullyLeft += ClientMessageReciever_OnRoomSuccessfullyLeft;
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

            if (_waitingRoom != null)
            {
                int startYPos = _roomStartYPos;
                foreach (var room in _waitingRoom.Rooms)
                {
                    GameRoomUIItem uiItem = new GameRoomUIItem
                    {
                        // Set Rect
                        Rect = new Rectangle(50, startYPos, 500, 50),
                        // Set Text
                        Text = room.RoomName + " : " + room.ConnectionCount + "/" + WaitingRoom.MAX_PEOPLE_PER_ROOM + " Players"
                    };

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
                }
            }
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            _device = graphicsDevice;

            _titlePosition = new Vector2(Width / 2, 0);
            _titlePosition.X -= (_font.MeasureString(_titleText).X / 2);

            _newRoomButtonPosition = new Vector2(Width / 2, Height - (_font.MeasureString(_newRoomButtonText).Y));
            _newRoomButtonPosition.X -= (_font.MeasureString(_newRoomButtonText).X / 2);
            _newRoomButtonRect = new Rectangle((int)_newRoomButtonPosition.X, (int)_newRoomButtonPosition.Y, 
                (int)_font.MeasureString(_newRoomButtonText).X, (int)_font.MeasureString(_newRoomButtonText).Y);
            _newRoomButtonColor = Color.Blue;
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            // Is it possible to create a new room? Must me less rooms than max and this client can't currently be in a room
            if (_roomUIItems.Count < Server.MAX_ROOMS && string.IsNullOrEmpty(_joinedRoomID))
            {
                // If mouse has been clicked
                if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_newRoomButtonRect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        // New room valid click
                        OnNewGameRoomClicked();
                    }
                } 
                else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // Else if mouse is hovering
                {
                    if (_newRoomButtonRect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        _newRoomButtonColor = Color.LightGreen;
                    }
                    else
                    {
                        _newRoomButtonColor = Color.Blue;
                    }
                }
            }
            else
            {
                _newRoomButtonColor = Color.Red;
            }

            // Check for click
            if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released && !_waitingForResponseFromServer)
            {
                // Check all ui rooms if any have been clicked on
                for (int i = 0; i < _roomUIItems.Count; i++)
                {
                    if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        if (string.IsNullOrEmpty(_joinedRoomID))
                        {
                            // Valid click on UI room
                            OnJoinGameRoom(_waitingRoom.Rooms[i].RoomID);
                            _waitingForResponseFromServer = true;
                        }
                        else
                        {
                            if (_waitingRoom.Rooms[i].RoomID == _joinedRoomID)
                            {
                                // ON LEAVE
                                OnLeaveGameRoom(_waitingRoom.Rooms[i].RoomID);
                                _waitingForResponseFromServer = true;
                            }
                        }
                    }
                }
            } 
            else if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                // Check all ui rooms if any have the mouse over it
                for (int i = 0; i < _roomUIItems.Count; i++)
                {
                    if (string.IsNullOrEmpty(_joinedRoomID))
                    {
                        if (_roomUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                        {
                            _roomUIItems[i].BorderColour = Color.LightGreen;
                        }
                        else
                        {
                            _roomUIItems[i].BorderColour = Color.Blue;
                        }
                    }
                    else 
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
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw title
            spriteBatch.DrawString(_font, _titleText, _titlePosition, Color.White);

            // Draw new room button
            spriteBatch.DrawString(_font, _newRoomButtonText, _newRoomButtonPosition, Color.White);
            Texture2D newRoomBtnTexture = new Texture2D(_device, _newRoomButtonRect.Width, _newRoomButtonRect.Height);
            newRoomBtnTexture.CreateBorder(1, _newRoomButtonColor);
            spriteBatch.Draw(newRoomBtnTexture, _newRoomButtonPosition, Color.White);

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
    }
}
