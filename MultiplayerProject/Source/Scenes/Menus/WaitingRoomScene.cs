using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace MultiplayerProject.Source
{
    public class LobbyUIItem
    {
        public Vector2 Position;
        public Rectangle Rect;
        public string Text;
        public Color BorderColour;
        public int BorderWidth;
    }

    public class WaitingRoomScene :  IScene
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private WaitingRoomInformation _waitingRoom;
        private List<LobbyUIItem> _lobbyUIItems;

        private int _joinedLobby;

        private SpriteFont _font;
        private GraphicsDevice _device;

        // Title
        private Vector2 _titlePosition;
        private const string _titleText = "WAITING ROOM";

        // New room button
        private Vector2 _newRoomButtonPosition;
        private Rectangle _newRoomButtonRect;
        private const string _newRoomButtonText = "CREATE NEW ROOM";

        // Lobbys
        private const int _lobbyStartYPos = 50;

        public WaitingRoomScene(int width, int height)
        {
            Width = width;
            Height = height;

            _lobbyUIItems = new List<LobbyUIItem>();
            _joinedLobby = -1;

            Client.OnWaitingRoomInformationRecieved += Client_OnWaitingRoomInformationRecieved;
        }

        private void Client_OnWaitingRoomInformationRecieved(WaitingRoomInformation waitingRoom)
        {
            _waitingRoom = waitingRoom;
            _lobbyUIItems.Clear();
            if (_waitingRoom != null)
            {
                int startYPos = _lobbyStartYPos;
                foreach (var lobby in _waitingRoom.Lobbies)
                {
                    LobbyUIItem uiItem = new LobbyUIItem();

                    // Set Rect
                    uiItem.Rect = new Rectangle(50, startYPos, 500, 50);
                    // Set Text
                    uiItem.Text = lobby.LobbyName + " : " + lobby.ConnectionCount + "/" + WaitingRoom.MAX_PEOPLE_PER_LOBBY + " Players";

                    Vector2 size = _font.MeasureString(uiItem.Text);
                    float xScale = (uiItem.Rect.Width / size.X);
                    float yScale = (uiItem.Rect.Height / size.Y);
                    float scale = Math.Min(xScale, yScale);

                    // Figure out the location to absolutely-center it in the boundaries rectangle.
                    int strWidth = (int)Math.Round(size.X * scale);
                    int strHeight = (int)Math.Round(size.Y * scale);

                    // Set Position
                    uiItem.Position = new Vector2();
                    uiItem.Position.X = (((uiItem.Rect.Width - strWidth) / 2) + uiItem.Rect.X);
                    uiItem.Position.Y = (((uiItem.Rect.Height - strHeight) / 2) + uiItem.Rect.Y);

                    // Set border options
                    uiItem.BorderColour = Color.Blue;
                    uiItem.BorderWidth = 1;

                    _lobbyUIItems.Add(uiItem);
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
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            // Check for click
            if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < _lobbyUIItems.Count; i++)
                {
                    if (_lobbyUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        
                    }
                }
            } 

            // Hover over effect
            if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < _lobbyUIItems.Count; i++)
                {
                    if (i != _joinedLobby)
                    {
                        if (_lobbyUIItems[i].Rect.Contains(inputInfo.CurrentMouseState.Position))
                        {
                            _lobbyUIItems[i].BorderColour = Color.LightGreen;
                        }
                        else
                        {
                            _lobbyUIItems[i].BorderColour = Color.Blue;
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
            newRoomBtnTexture.CreateBorder(1, Color.Blue);
            spriteBatch.Draw(newRoomBtnTexture, _newRoomButtonPosition, Color.White);

            if (_lobbyUIItems.Count != 0)
            {
                for (int i = 0; i < _lobbyUIItems.Count; i++)
                {
                    // Draw the string to the sprite batch!
                    spriteBatch.DrawString(_font, _lobbyUIItems[i].Text, _lobbyUIItems[i].Position, Color.White);

                    Texture2D texture = new Texture2D(_device, _lobbyUIItems[i].Rect.Width, _lobbyUIItems[i].Rect.Height);
                    texture.CreateBorder(_lobbyUIItems[i].BorderWidth, _lobbyUIItems[i].BorderColour);
                    spriteBatch.Draw(texture, _lobbyUIItems[i].Position, Color.White);
                }
            }
        }
    }
}
